using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// #my_code - Comportamento individual de cada NPC, incluindo lógica de patrulha, deteção e diálogo
public class NPCScript : InteractableObject {

    public enum NPCType { Colleague, Boss, Receptionist, Guard, Visitor } // tipos diferentes de NPC no jogo
    public enum NPCState { Idle, Patrol, Attention, Investigate, Chase } // possíveis estados em que poderão estar

    // o tipo de NPC dita as rotas que ele pode seguir
    public NPCType npcType = NPCType.Colleague;

    // se a flag for false o NPC fica em Idle quando o stealth state regressa ao normal, foi pensado para NPCs com posição fixa, como a rececionista
    private bool isPatrolling = true;

    // campo de visão dos NPC
    private float fovAngle = 90f;
    private float fovRange = 15f;

    // A lanterna liga um buff no FOV dos NPCs durante a noite, este valor soma-se ao fovRange base para aumentar o alcance da visão deles
    [SerializeField] private float lightBonusRange = 10f;

    // distância máxima em que os guardas conseguem ouvir passos, comparamos isto com o GetNoiseRadius do PlayerController que muda consoante a postura do jogador
    [SerializeField] private float hearingRadius = 8f;

    private Transform playerTransform;

    // guardamos o último sítio onde o jogador foi visto ou ouvido para mandar o NPC investigar lá diretamente e não vaguear
    private Vector3 lastKnownPlayerPosition;

    private NavMeshAgent agent;
    private Animator animator;

    private Coroutine patrolCoroutine;
    // o NPCManager precisa de saber em que piso o NPC está para lhe dar rotas apropriadas a essa área
    public int currentFloor = 0;
    private PatrolRoute currentRoute;
    public Transform homeBase;
    [SerializeField] private bool despawnAtEnd = false;

    // esta rota é utilizada para o NPC percorrer uma rota inicial antes de lhe começarem a ser atribuídas rotas ao acaso
    [HideInInspector] public PatrolRoute startRoute; // usado por exemplo quando os NCP spawnam no piso dos quartos para vaguearem antes direm pros seus quartos

    // se o NPC tiver uma rota fixa atribuída ignoramos o sistema aleatório de rotas, pois o jogador faz sempre este caminho
    [HideInInspector] public PatrolRoute assignedRoute; // usado por exemplo para os NPC que spawnam na receção terem dir para os elevadores

    // o spawner precisa de saber quando o NPC é destruído para gerir o limite de NPC spawnados (caso seja spawnado por um)
    [HideInInspector] public NPCSpawner spawner;

    // usado pelo NPCManager para bloquear a saída da rececionista/guarda até estar na posição de origem para apenas um NPC estar a descansar
    // enquanto o outro continua a "trabalhar"
    [HideInInspector] public bool isAtHome;
    // o NPCManager consulta isto para garantir que não há dois guardas a descansar ao mesmo tempo
    [HideInInspector] public bool isResting;

    // identifica departamentos específicos, 0 significa sem departamento, é usado para rotas específicas para NPC de certos departamentos, nomeadamente comer e beber água dentro do seu dep
    [SerializeField] private int departmentID = 0; 

    // usamos esta string para validar entregas de documentos, se o ID bater certo com o correctRecipientID da task o documento foi bem entregue
    [SerializeField] private string npcID;

    // associa os dados da árvore de diálogo ao NPC
    [Header("Diálogo")]
    [SerializeField] private NPCDialogueData dialogueData;

    [Header("Cartões")]
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

        // separamos o check de áudio dos guardas numa corrotina independente para ser mais otimizado
        if (npcType == NPCType.Guard)
            StartCoroutine(NoiseCheckRoutine());

        // se houver uma rota de arranque configurada lançamos logo isso, senão atiramos a máquina de estados para a patrulha geral
        if (startRoute != null)
            StartCoroutine(RunStartRoute());
        else
            SetState(NPCState.Patrol);
    }

    // tratamos aqui a prioridade de entregar documentos sobre iniciar a conversa normal mal o jogador interaja com o jogador
    public override void Interact() {
        // se o jogador possuir um documento e a tarefa de entregar o mesmo está ativada então entregamos o mesmo a este NPC
        if (PlayerController.Instance.heldDocument != null && TaskManager.Instance.HasActiveTask("Entregar documento")) {
            TryDeliverDocument();
            return;
        }

        if (dialogueData == null) {
            Debug.LogWarning($"[NPCScript] {objectName} não tem NPCDialogueData atribuído.");
            return;
        }

        // se pudermos interagir com ele, paramos a patrulha do NavMeshAgent para não se ir embora a meio da conversa
        if (patrolCoroutine != null) {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        agent.isStopped = true;
        animator.SetInteger("State", 0);

        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // começamos o diálogo
        DialogueManager.Instance.OpenDialogue(dialogueData);
        DialogueManager.Instance.OnDialogueClose += ResumeAfterDialogue;
    }

    // lógica da interação de entregar documento, valida pelo ID e aumenta a suspeita se for a pessoa errada
    private void TryDeliverDocument() {
        DocumentTaskData doc = PlayerController.Instance.heldDocument;
        bool correct = !string.IsNullOrEmpty(npcID) && doc.correctRecipientID == npcID;

        TaskManager.Instance.CompleteTask("Entregar documento", correct);
        PlayerController.Instance.heldDocument = null;

        if (correct) {
            Debug.Log($"[NPCScript] Documento entregue corretamente a '{objectName}'.");
        } else {
            Debug.Log($"[NPCScript] Destinatário errado! Documento entregue a '{objectName}' em vez do destinatário correto.");
            SuspicionManager.Instance.IncreaseSuspicion(1.5f, GetInstanceID(), SuspicionManager.SuspicionSource.DocumentMisfiled);
        }
    }

    // removemos o listener da framework de diálogos para a retoma do estado não duplicar comportamentos
    private void ResumeAfterDialogue() {
        DialogueManager.Instance.OnDialogueClose -= ResumeAfterDialogue;
        agent.isStopped = false;
        SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
    }

    // quando o NPC tem uma rota inicial metemo-lo a percorre-la, mas baralhamos os waypoints à mesma
    private IEnumerator RunStartRoute() {
        currentRoute = startRoute;
        currentState = NPCState.Patrol;
        animator.SetInteger("State", 1);
        agent.isStopped = false;

        Transform[] shuffled = (Transform[])currentRoute.waypoints.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--) {
            int rand = Random.Range(0, i + 1);
            Transform temp = shuffled[i];
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        // passamos wwaypoints à frente para imprevisibilidade
        for (int i = 0; i < currentRoute.waypoints.Length; i++) {
            if (Random.value < 0.75f)
                continue;

            agent.SetDestination(shuffled[i].position);

            while (agent.pathPending || agent.remainingDistance >= 0.5f)
                yield return null;

            if (currentRoute.waitTimePerWaypoint > 0f) {
                animator.SetInteger("State", 0);
                yield return new WaitForSeconds(TimeManager.Instance.ToRealSeconds(currentRoute.waitTimePerWaypoint));
                animator.SetInteger("State", 1);
            }
        }

        // depois de fazer a rota começam a patrulhar
        EnterPatrol();
    }

    private void Update() {
        if (currentState == NPCState.Investigate) {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                // quando os NPC estão a investigar deslocam-se para o último local onde esteve o jogador, quando chegarem podem voltar ao estado anterior
                SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
        }

        // se estiverem em CHASE é perseguirem sempre o jogador
        if (currentState == NPCState.Chase)
            agent.SetDestination(playerTransform.position);
    }

    void OnDestroy() {
        NPCManager.Instance.UnregisterNPC(this);
        SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());

        if (spawner != null)
            spawner.OnNPCDestroyed();
    }

    private void SetState(NPCState newState) {
        if (currentState == newState)
            return;

        // damos reset à corrotina antes da mudança senão arriscamos os NPC irem para os waypoints enquanto estão no estado de chase
        if (patrolCoroutine != null) {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }

        currentState = newState;

        // dependendo do estado mudamos as suas animações
        int animState;
        switch (newState) {
            case NPCState.Chase:
                animState = 2; // correr
                break;
            case NPCState.Patrol:
            case NPCState.Investigate:
                animState = 1; // andar
                break;
            default:
                animState = 0; // idle
                break;
        }

        animator.SetInteger("State", animState);

        switch (newState) {
            case NPCState.Idle: EnterIdle(); break;
            case NPCState.Patrol: EnterPatrol(); break;
            case NPCState.Attention: EnterAttention(); break;
            case NPCState.Investigate: EnterInvestigate(); break;
            case NPCState.Chase: EnterChase(); break;
        }
    }

    private void EnterIdle() {
        agent.isStopped = true;
    }

    private void EnterPatrol() {
        // os NPC rececionistas não fazem patrol se pelo menos uma já estiver numa rota de descansar
        if (npcType == NPCType.Receptionist && !NPCManager.Instance.CanReceptionistLeave()) {
            SetState(NPCState.Idle);
            return;
        }

        // se o NPC não tiver uma rota específica a fazer sempre, apanha uma ao acaso
        if (assignedRoute != null) {
            currentRoute = assignedRoute;

        } else {
            currentRoute = NPCManager.Instance.GetRandomRoute(npcType, currentFloor, departmentID, currentRoute);

            // se o NPC for um guarda e já estiver na rota de descanso e não puder entrar nela (não vai porque já está nela) então excluímos as rotas de descanso
            if (npcType == NPCType.Guard && currentRoute.isRestRoute && !NPCManager.Instance.CanGuardRest()) {
                currentRoute = NPCManager.Instance.GetRandomRoute(npcType, currentFloor, departmentID, currentRoute, excludeRest: true);
            }
        }

        isResting = currentRoute.isRestRoute;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    // agora que o NPC já tem atribuída uma rota selecionada ao acaso, poderá começar a percorre-la
    private IEnumerator PatrolRoutine() {
        agent.isStopped = false;

        do {
            // baralhamos os waypoints todos para imprevisibilidade
            Transform[] shuffled = (Transform[])currentRoute.waypoints.Clone();
            for (int i = shuffled.Length - 1; i > 0; i--) {
                int rand = Random.Range(0, i + 1);
                Transform temp = shuffled[i];
                shuffled[i] = shuffled[rand];
                shuffled[rand] = temp;
            }

            // enquanto o NPC estiver a percorrer waypoints mantemo-lo a andar, se já tiver chegado fica idle o tempo definido na rota em questão
            for (int i = 0; i < currentRoute.waypoints.Length; i++) {
                animator.SetInteger("State", 1);
                agent.SetDestination(shuffled[i].position);

                while (agent.pathPending || agent.remainingDistance >= 0.5f)
                    yield return null;

                if (currentRoute.waitTimePerWaypoint > 0f) {
                    animator.SetInteger("State", 0);
                    yield return new WaitForSeconds(TimeManager.Instance.ToRealSeconds(currentRoute.waitTimePerWaypoint));
                    animator.SetInteger("State", 1);
                }
            }

        } while (currentRoute.loopWaypoints && currentState == NPCState.Patrol); // apenas se tiver no estado de patrol E se for para fazer a rota em loop

        // se não for em loop verificamos se o NPC pode voltar ao início
        if (currentRoute.returnHome) {
            animator.SetInteger("State", 1);
            agent.SetDestination(homeBase.position);

            while (agent.pathPending || agent.remainingDistance >= 0.5f)
                yield return null;

            animator.SetInteger("State", 0);
        }

        // se ainda estiver em patrol e tiver chegado ao último waypoint verificamos se é destruído (por exemplo chegou ao elevador de volta)
        // ou percorre outra rota
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

    // isto verifica o campo de vista do NPC, só é ativado se o jogador estiver no mesmo piso, senão não faz sentido verificar
    // é usado para ver se o jogador está num local onde não devia estar
    private IEnumerator FOVCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (true) {
            if (!isFloorActive) { 
                yield return wait; 
                continue; 
            }

            if (PlayerController.Instance.inSusPlace && IsPlayerInFOV()) {
                lastKnownPlayerPosition = playerTransform.position;
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                float level = GetSuspicionLevelByDistance(dist); // aumentamos a suspeita de acordo com o quão perto o jogador está do NPC

                // se for de noite e o jogador tiver a sua lanterna ligada aumentamos a suspeita ainda mais
                if (TimeManager.Instance.isNight && FlashlightController.Instance.isOn) {
                    level = Mathf.Min(3f, level + 1f);
                }

                // aqui atribuímos a suspeita calculada no suspicionManager e dizemos que é este NCP que está a causar o aumento da suspeita
                if (level > 0)
                    SuspicionManager.Instance.IncreaseSuspicion(level, GetInstanceID(), SuspicionManager.SuspicionSource.NPCSight);
            }

            yield return wait;
        }
    }

    // calcula os vetores com o campo de visão atual, expandimos o raio máximo se a lanterna do jogador estiver ativada à noite porque a luz chama à atenção
    private bool IsPlayerInFOV() {
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float dist = dir.magnitude;

        float effectiveRange = fovRange;
        if (TimeManager.Instance.isNight && FlashlightController.Instance.isOn) {
            effectiveRange += lightBonusRange;
        }

        if (dist > effectiveRange)
            return false;

        float angle = Vector3.Angle(forward, dir.normalized);

        return angle <= fovAngle / 2f;
    }

    // aumentamos a suspeita de acordo a distância para dar uma hipótese do jogador fugir se estiver nos limites da visão do guarda
    private float GetSuspicionLevelByDistance(float distance) {
        float third = fovRange / 3f;
        if (distance < third)
            return 2f;

        if (distance < third * 2f)
            return 1.5f;

        return 1f;
    }

    // corre um intervalo maior que a rotina visual mas só reage se o jogador fizer muito barulho e se o guarda não estiver já a correr para o problema
    private IEnumerator NoiseCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        // e portanto só faz sentido verificar se o jogador também estiver no piso, senão estamos a gastar recursos
        while (true) {
            if (!isFloorActive) { 
                yield return wait; 
                continue; 
            }

            if (TimeManager.Instance.isNight && PlayerController.Instance.IsPlayerMoving()) {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                float playerNoiseRadius = PlayerController.Instance.GetNoiseRadius();

                if (dist <= playerNoiseRadius && dist <= hearingRadius && currentState != NPCState.Investigate && currentState != NPCState.Chase) {
                    lastKnownPlayerPosition = playerTransform.position;
                    Debug.Log($"[{objectName}] Ouviu o jogador a {dist:F1}m! A investigar.");

                    SuspicionManager.Instance.AddInstantSuspicion(0.05f);

                    SetState(NPCState.Investigate);
                }
            }

            yield return wait;
        }
    }

    // sempre que a suspeita é alterada todos os NPC verificam o estado da suspeita para poderem reagir de acordo com o novo estado
    public void OnGlobalSuspicionChanged(SuspicionManager.SuspicionState state) {
        switch (state) {
            case SuspicionManager.SuspicionState.None:
                SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
                break;
            case SuspicionManager.SuspicionState.Attention:
                SetState(NPCState.Attention);
                break;
            case SuspicionManager.SuspicionState.Investigation:
                if (npcType == NPCType.Guard) // apenas os guardas investigam
                    SetState(NPCState.Investigate);
                break;
            case SuspicionManager.SuspicionState.Expulsion:
                SetState(NPCState.Chase);
                break;
        }
    }

    // usado para forçar a rota das reuniões quando o evento é triggered
    public void ForceRoute(PatrolRoute route) {
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        currentRoute = route;
        currentState = NPCState.Patrol;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    // se o jogador estiver no mesmo piso que este NPC então eles fazem as suas rotas normalmente, senão para de aumentar suspeita (se estivesse) e para de andar
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