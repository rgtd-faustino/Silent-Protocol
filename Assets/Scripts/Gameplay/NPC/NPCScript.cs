using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : InteractableObject {

    public enum NPCType { Colleague, Boss, Receptionist, Guard, Visitor }
    public enum NPCState { Idle, Patrol, Attention, Investigate, Chase }

    // determina quais rotas pode usar e comportamentos especiais
    public NPCType npcType = NPCType.Colleague;

    // se false o NPC fica Idle em vez de Patrol quando o estado global volta ao normal
    // usado para NPCs que têm posições fixas (como a rececionista na secretária)
    private bool isPatrolling = true;

    private float fovAngle = 90f;
    private float fovRange = 15f;

    // quando a lanterna do jogador está ligada os NPCs veem mais longe à noite
    // este valor é somado ao fovRange normal para calcular o alcance efetivo
    [SerializeField] private float lightBonusRange = 10f;

    // raio máximo dentro do qual o guarda ouve o jogador à noite
    // ou seja, os restantes tipos de NPC ignoram o ruído.
    // o valor é comparado com o PlayerController.GetNoiseRadius(), que varia
    // consoante o jogador está a correr (10 m), a andar (5 m) ou agachado (2 m).
    [SerializeField] private float hearingRadius = 8f;

    private Transform playerTransform;

    // usado pelo estado Investigate e é atualizada sempre que o jogador está no FOV ou é ouvido
    private Vector3 lastKnownPlayerPosition;

    private NavMeshAgent agent;
    private Animator animator;

    private Coroutine patrolCoroutine;
    // usado pelo NPCManager para atribuir rotas
    public int currentFloor = 0;
    private PatrolRoute currentRoute;
    public Transform homeBase;
    [SerializeField] private bool despawnAtEnd = false;

    // rota corrida uma única vez ao spawnar, antes de entrar no loop normal, usado pelos colegas nos apartamentos para vaguearem antes direm para os quartos
    [HideInInspector] public PatrolRoute startRoute;

    // rota fixa, este NPC ignora o GetRandomRoute e usa sempre esta
    // usado pelo npc colega com caminho sempre igual porta+secretária+elevador
    [HideInInspector] public PatrolRoute assignedRoute;

    // se a referencia não for null, avisa o spawner quando o npc é destruído
    // para que possa contar quantos NPCs ativos tem nele
    [HideInInspector] public NPCSpawner spawner;

    // lido pelo NPCManager para saber se a rececionista pode sair
    [HideInInspector] public bool isAtHome;
    // lido pelo NPCManager para que só um guarda descansa de cada vez
    [HideInInspector] public bool isResting;

    [SerializeField] private int departmentID = 0; // por causa dos colegas rececionistas e chefes no piso executivo (0 = sem departamento)

    // identificador unico deste NPC, usado para comparar com o DocumentTaskData.correctRecipientID na task "Entregar documento"
    [Header("Entrega de Documentos")]
    [SerializeField] private string npcID;

    // arrastar o NPCDialogueData correto aqui no Inspector
    // no futuro pode ser atribuído individualmente por NPC em vez de por tipo
    [Header("Diálogo")]
    [SerializeField] private NPCDialogueData dialogueData;

    [Header("Cards (CanvasGroup em cada card central)")]
    [SerializeField] private CanvasGroup mainMenuCard;
    [SerializeField] private CanvasGroup charCreationCard;
    [SerializeField] private CanvasGroup pauseCard;

    private NPCState currentState;
    private bool isFloorActive = true;


    protected override void Awake() {
        base.Awake();
        objectName = "Personagem";
        tooltipMessage = "E para falar";
    }

    void Start() {
        playerTransform = NPCManager.Instance.player;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        NPCManager.Instance.RegisterNPC(this);
        isAtHome = homeBase != null && Vector3.Distance(transform.position, homeBase.position) < 1f;

        StartCoroutine(FOVCheckRoutine());

        // guardsreagem a ruído — corrotina separada para não bloquear o FOV check
        if (npcType == NPCType.Guard)
            StartCoroutine(NoiseCheckRoutine());

        // antes de começarmos o patrol vemos se o npc tem uma rota inicial a fazer antes de começar o loop
        if (startRoute != null)
            StartCoroutine(RunStartRoute());
        else
            SetState(NPCState.Patrol);
    }

    // chamado pelo CameraScript quando o jogador carrega E apontado para este NPC
    public override void Interact() {
        // se o jogador tiver um documento na mao E a task de entrega estiver ativa,
        // a interacao tenta entregar o documento em vez de abrir dialogo
        // (assim continua a poder falar normalmente com NPCs em dias so de Arquivar documento)
        if (PlayerController.Instance.heldDocument != null && TaskManager.Instance.HasActiveTask("Entregar documento")) {
            TryDeliverDocument();
            return;
        }

        if (dialogueData == null) {
            Debug.LogWarning($"[NPCScript] {objectName} não tem NPCDialogueData atribuído.");
            return;
        }

        // pausa o NPC para ele não andar enquanto fala
        if (patrolCoroutine != null) {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        agent.isStopped = true;
        animator.SetInteger("State", 0); // idle

        // vira o NPC para o jogador
        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        DialogueManager.Instance.OpenDialogue(dialogueData);

        // quando o diálogo fechar, retoma o patrol
        DialogueManager.Instance.OnDialogueClose += ResumeAfterDialogue;
    }

    // tenta entregar o documento que o jogador tem na mao a este NPC
    // correto ou nao, a task fica completa; a penalidade vem pelo SuspicionManager (mesma logica do ArchiveScript)
    private void TryDeliverDocument() {
        DocumentTaskData doc = PlayerController.Instance.heldDocument;
        bool correct = !string.IsNullOrEmpty(npcID) && doc.correctRecipientID == npcID;

        TaskManager.Instance.CompleteTask("Entregar documento", correct);
        PlayerController.Instance.heldDocument = null;

        if (correct) {
            Debug.Log($"[NPCScript] Documento entregue corretamente a '{objectName}'.");
        } else {
            Debug.Log($"[NPCScript] Destinatario errado! Documento entregue a '{objectName}' em vez do destinatario correto.");
            // entrega a pessoa errada levanta suspeita imediata, tal como um documento mal arquivado
            SuspicionManager.Instance.IncreaseSuspicion(1.5f, GetInstanceID(), SuspicionManager.SuspicionSource.DocumentMisfiled);
        }
    }

    // retoma o movimento após fechar o diálogo — dessubscreve logo para não acumular
    private void ResumeAfterDialogue() {
        DialogueManager.Instance.OnDialogueClose -= ResumeAfterDialogue;
        agent.isStopped = false;
        SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
    }

    // o método é para fazer os NPCs que tenham uma rota pre definida a sigam sempre antes direm para as rotas normais
    private IEnumerator RunStartRoute() {
        currentRoute = startRoute;
        currentState = NPCState.Patrol;
        animator.SetInteger("State", 1); // andar
        agent.isStopped = false;

        Transform[] shuffled = (Transform[])currentRoute.waypoints.Clone();  // clone para não alterar a lista original just in case
        for (int i = shuffled.Length - 1; i > 0; i--) {
            int rand = Random.Range(0, i + 1);
            Transform temp = shuffled[i];
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        for (int i = 0; i < currentRoute.waypoints.Length; i++) {
            // chance de skipar este waypoint
            if (Random.value < 0.75f)
                continue;

            agent.SetDestination(shuffled[i].position);

            // esperamos que o npc chegue ao waypoint (path pending porque é preciso algumas frames para computar e até la a remaining distance é 0)
            while (agent.pathPending || agent.remainingDistance >= 0.5f)
                yield return null;

            if (currentRoute.waitTimePerWaypoint > 0f) {
                animator.SetInteger("State", 0); // idle
                // fazemos o NPC ficar algum tempo parado no waypoint antes de seguir para o prox
                yield return new WaitForSeconds(TimeManager.Instance.ToRealSeconds(currentRoute.waitTimePerWaypoint));
                animator.SetInteger("State", 1);
            }

        }

        EnterPatrol();
    }

    private void Update() {
        if (currentState == NPCState.Investigate) {
            // quando o investigate é o estado atual o NPC vai para a ultima pos do jogador
            // então quando lá chegar volta se a meter os estados normais
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
        }

        // no chase é preciso ir atrás do jogador a cada frame
        if (currentState == NPCState.Chase)
            agent.SetDestination(playerTransform.position);
    }

    void OnDestroy() {
        NPCManager.Instance.UnregisterNPC(this);
        SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());

        // se foi spawnado avisamos para o contador diminuir de npcs ativos
        if (spawner != null)
            spawner.OnNPCDestroyed();
    }


    private void SetState(NPCState newState) {
        if (currentState == newState)
            return;

        // cancela a PatrolRoutine que esteja a correr antes de mudar de estado (para só haver uma) e assim
        // faz com que o NPC não continue a andar para waypoints enquanto está em Chase ou Attention
        if (patrolCoroutine != null) {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }

        currentState = newState;

        int animState;
        switch (newState) {
            case NPCState.Chase:
                animState = 2; // correr (tecnicamente andar depressa)
                break;

            case NPCState.Patrol:
                animState = 1; // andar
                break;

            case NPCState.Investigate:
                animState = 1; // andar
                break;

            default:
                animState = 0; // idle
                break;
        }

        animator.SetInteger("State", animState);

        switch (newState) {
            case NPCState.Idle:
                EnterIdle();
                break;

            case NPCState.Patrol:
                EnterPatrol();
                break;

            case NPCState.Attention:
                EnterAttention();
                break;

            case NPCState.Investigate:
                EnterInvestigate();
                break;

            case NPCState.Chase:
                EnterChase();
                break;
        }
    }

    private void EnterIdle() {
        agent.isStopped = true;
    }

    private void EnterPatrol() {
        // so uma rececionista é que pode estar a vaguear ao mesmo tempo
        if (npcType == NPCType.Receptionist && !NPCManager.Instance.CanReceptionistLeave()) {
            SetState(NPCState.Idle);
            return;
        }

        // fazemos a rota pre definida que é sempre igual se houver
        if (assignedRoute != null) {
            currentRoute = assignedRoute;

        } else {
            // apanhamos a próxima rota
            currentRoute = NPCManager.Instance.GetRandomRoute(npcType, currentFloor, departmentID, currentRoute);

            // se um guarda já estiver em descanso então pede se outra rota mas que não seja de descanso porque só pode haver um a descansar ao msm tempo
            if (npcType == NPCType.Guard && currentRoute.isRestRoute && !NPCManager.Instance.CanGuardRest()) {
                currentRoute = NPCManager.Instance.GetRandomRoute(npcType, currentFloor, departmentID, currentRoute, excludeRest: true);
            }
        }

        isResting = currentRoute.isRestRoute;
        patrolCoroutine = StartCoroutine(PatrolRoutine()); // começamos a próxima corrotina
    }

    private IEnumerator PatrolRoutine() {
        agent.isStopped = false;

        do {
            // se as rotas forem sempres as mesmas torna o jogo previsivel e aborrecido para o jogador então damos shuffle
            // fazemos dentro do loop para que no fim de cada loop dê shuffle outra vez
            // não baralhamos a runstartroute porque tem ordem intencional
            Transform[] shuffled = (Transform[])currentRoute.waypoints.Clone();  // clone para não alterar a lista original just in case
            for (int i = shuffled.Length - 1; i > 0; i--) {
                int rand = Random.Range(0, i + 1);
                Transform temp = shuffled[i];
                shuffled[i] = shuffled[rand];
                shuffled[rand] = temp;
            }

            for (int i = 0; i < currentRoute.waypoints.Length; i++) {
                animator.SetInteger("State", 1); // metemos o npc a andar
                agent.SetDestination(shuffled[i].position);

                // esperamos que chegue ao proximo waypoint
                while (agent.pathPending || agent.remainingDistance >= 0.5f)
                    yield return null;

                // se houver tempo de espera no waypoint metemos idle no npc
                if (currentRoute.waitTimePerWaypoint > 0f) {
                    animator.SetInteger("State", 0); // idle
                    // fazemos o NPC ficar algum tempo parado no waypoint antes de seguir para o prox
                    yield return new WaitForSeconds(TimeManager.Instance.ToRealSeconds(currentRoute.waitTimePerWaypoint));
                    animator.SetInteger("State", 1);
                }
            }

            // usamos do while porque só porque não seja um loop não quer dizer que o NPC não 
            // se mova entre waypoints, então caso não seja o npc avança todos e se for repete tudo no fim
        } while (currentRoute.loopWaypoints && currentState == NPCState.Patrol);

        // os npcs que tiverem de voltar a casa voltam como a rececionista
        if (currentRoute.returnHome) {
            animator.SetInteger("State", 1);
            agent.SetDestination(homeBase.position);

            while (agent.pathPending || agent.remainingDistance >= 0.5f)
                yield return null;

            animator.SetInteger("State", 0);
        }

        // por ex quando os colegas chegam ao elevador ou os visitantes à saída
        if (currentState == NPCState.Patrol) {
            if (despawnAtEnd)
                Destroy(gameObject);
            else
                EnterPatrol();
        }
    }

    private void EnterAttention() {
        agent.isStopped = true;
    }

    private void EnterInvestigate() {
        agent.isStopped = false;
        agent.SetDestination(lastKnownPlayerPosition);
    }

    private void EnterChase() {
        agent.isStopped = false;
    }


    // verifica 10 vezes por segundo, suficiente para ser responsivo sem usar muito o CPU
    // a condição PlayerController.Instance.inSusPlace verifica se o jogador está numa zona suspeita
    // à noite, se a lanterna estiver ligada, o nível de suspeita gerado por segundo é aumentado em +1
    private IEnumerator FOVCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (true) {
            // se o jogador não estiver no piso não precisamos de estar a ver isto nesses NPCs
            if (!isFloorActive) { 
                yield return wait; 
                continue; 
            }

            if (PlayerController.Instance.inSusPlace && IsPlayerInFOV()) {
                lastKnownPlayerPosition = playerTransform.position;
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                float level = GetSuspicionLevelByDistance(dist);

                // lanterna ligada à noite -> nível de suspeita mais alto porque o jogador é um alvo luminoso no escuro
                if (TimeManager.Instance.isNight &&
                    FlashlightController.Instance != null &&
                    FlashlightController.Instance.isOn) {
                    level = Mathf.Min(3f, level + 1f);
                }

                if (level > 0)
                    SuspicionManager.Instance.IncreaseSuspicion(level, GetInstanceID(), SuspicionManager.SuspicionSource.NPCSight);
            }

            yield return wait;
        }
    }

    // verifica se o jogador está dentro do cone de visão deste NPC através da distância e ângulo.
    // à noite com lanterna ligada o alcance efetivo aumenta (lightBonusRange).
    private bool IsPlayerInFOV() {
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float dist = dir.magnitude;

        // à noite com lanterna o NPC vê mais longe porque a luz ilumina o próprio jogador
        float effectiveRange = fovRange;
        if (TimeManager.Instance.isNight && FlashlightController.Instance.isOn) {
            effectiveRange += lightBonusRange;
        }

        if (dist > effectiveRange)
            return false;

        float angle = Vector3.Angle(forward, dir.normalized);

        return angle <= fovAngle / 2f;
    }

    // quanto mais perto, maior o nível de suspeita (1, 1.5 ou 2).
    // estes valores são usados pelo SuspicionManager como multiplicador da velocidade de subida da barra.
    private float GetSuspicionLevelByDistance(float distance) {
        float third = fovRange / 3f;
        if (distance < third)
            return 2f;

        if (distance < third * 2f)
            return 1.5f;

        return 1f;
    }

    // a cada 0.2s tal como o FOV, o guarda só reage à noite e se o jogador se estiver a mexer, estiver dentro do seu raio de ruído e não estiver já em investigate ou chase
    // porque evita interrupções desnecessárias
    // se ouvir o jogador, o guarda vai investigar a última posição conhecida (EnterInvestigate).
    private IEnumerator NoiseCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true) {
            // se o jogador não estiver no piso não é preciso cada NPC estar a procurar por barulho
            if (!isFloorActive) { 
                yield return wait; 
                continue; 
            }


            if (TimeManager.Instance.isNight && PlayerController.Instance.IsPlayerMoving()) {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                float playerNoiseRadius = PlayerController.Instance.GetNoiseRadius();

                // o guarda ouve o jogador se estiver dentro do raio de ruído
                // e também dentro do seu próprio alcance de audição (hearingRadius)
                if (dist <= playerNoiseRadius && dist <= hearingRadius && currentState != NPCState.Investigate && currentState != NPCState.Chase) {
                    lastKnownPlayerPosition = playerTransform.position;
                    Debug.Log($"[{objectName}] Ouviu o jogador a {dist:F1}m! A investigar.");

                    // leve subida instantânea pequena na suspeita para dar feedback ao jogador de que foi ouvido
                    SuspicionManager.Instance.AddInstantSuspicion(0.05f);

                    SetState(NPCState.Investigate);
                }
            }

            yield return wait;
        }
    }


    // chamado pelo NPCManager quando o estado global de suspeita muda, cada NPC decide a sua própria reação com base no seu tipo.
    // guardas são mais proativos —> entram em Investigate na fase de investigação enquanto colegas e outros só reagem a Attention e Expulsion.
    public void OnGlobalSuspicionChanged(SuspicionManager.SuspicionState state) {
        switch (state) {
            case SuspicionManager.SuspicionState.None:
                SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
                break;

            case SuspicionManager.SuspicionState.Attention:
                SetState(NPCState.Attention);
                break;

            case SuspicionManager.SuspicionState.Investigation:
                if (npcType == NPCType.Guard)
                    SetState(NPCState.Investigate);
                break;

            case SuspicionManager.SuspicionState.Expulsion:
                SetState(NPCState.Chase);
                break;
        }
    }

    // para o NPCManager alterar o modo de patrulha individualmente ou em grupo (via SetAllPatrolling)
    public void SetPatrolling(bool patrolling) {
        isPatrolling = patrolling;

        if (patrolling && currentState == NPCState.Idle)
            SetState(NPCState.Patrol);
        else if (!patrolling)
            SetState(NPCState.Idle);
    }

    // para os trabalhadores e chefes irem para o cubiculo de reunioes de vez em quando, todos ao mesmo tempo
    public void ForceRoute(PatrolRoute route) {
        // interrompemos a corrotina atual de patrulha caso haja para irem todos para a reunião
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        currentRoute = route;
        currentState = NPCState.Patrol;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    public bool IsPlayerVisible() {
        return PlayerController.Instance.inSusPlace && IsPlayerInFOV();
    }

    public void SetFloorActive(bool active) {
        isFloorActive = active;

        if (active) {
            agent.isStopped = false;
            currentState = NPCState.Idle;
            SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);

        } else {
            SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());

            if (patrolCoroutine != null) {
                StopCoroutine(patrolCoroutine);
                patrolCoroutine = null;
            }

            agent.isStopped = true;
            animator.SetInteger("State", 0);
        }
    }
}