using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// #my_code - Comportamento individual de cada NPC, incluindo lógica de patrulha, deteção e diálogo
public class NPCScript : InteractableObject {

    public enum NPCType { Colleague, Boss, Receptionist, Guard, Visitor }
    public enum NPCState { Idle, Patrol, Attention, Investigate, Chase }

    // O tipo de NPC dita as rotas que ele pode seguir e certos comportamentos específicos geridos pelo NPCManager.
    public NPCType npcType = NPCType.Colleague;

    // Se a flag for false o NPC fica em Idle quando o stealth state regressa ao normal. Foi pensado para NPCs com posição fixa, como a rececionista.
    private bool isPatrolling = true;

    private float fovAngle = 90f;
    private float fovRange = 15f;

    // A lanterna liga um buff no FOV dos NPCs durante a noite. Este valor soma-se ao fovRange base para aumentar o alcance da visão deles.
    [SerializeField] private float lightBonusRange = 10f;

    // Distância máxima em que os guardas conseguem ouvir passos. Comparamos isto com o GetNoiseRadius do PlayerController que muda consoante a postura do jogador.
    [SerializeField] private float hearingRadius = 8f;

    private Transform playerTransform;

    // Guardamos o último sítio onde o jogador foi visto ou ouvido para mandar o NPC investigar lá diretamente e não vaguear.
    private Vector3 lastKnownPlayerPosition;

    private NavMeshAgent agent;
    private Animator animator;

    private Coroutine patrolCoroutine;
    // O NPCManager precisa de saber em que piso o NPC está para lhe dar rotas apropriadas a essa área.
    public int currentFloor = 0;
    private PatrolRoute currentRoute;
    public Transform homeBase;
    [SerializeField] private bool despawnAtEnd = false;

    // Usamos esta rota apenas uma vez no spawn para simular comportamentos iniciais antes do loop principal, tipo andar um bocado pelos apartamentos.
    [HideInInspector] public PatrolRoute startRoute;

    // Se o NPC tiver uma rota fixa atribuída ignoramos o sistema aleatório de rotas. O gajo faz sempre este caminho.
    [HideInInspector] public PatrolRoute assignedRoute;

    // O spawner precisa de saber quando o NPC é destruído para gerir o limite de entidades ativas na cena.
    [HideInInspector] public NPCSpawner spawner;

    // Usado pelo NPCManager para bloquear a saída da rececionista até ela estar na posição de origem.
    [HideInInspector] public bool isAtHome;
    // O NPCManager consulta isto para garantir que não há dois guardas a descansar ao mesmo tempo.
    [HideInInspector] public bool isResting;

    // Identifica departamentos específicos. 0 significa sem departamento, muito útil para filtrar NPCs genéricos no piso executivo.
    [SerializeField] private int departmentID = 0;

    // Usamos esta string para validar entregas. Se o ID bater certo com o correctRecipientID da task o documento foi bem entregue.
    [Header("Entrega de Documentos")]
    [SerializeField] private string npcID;

    // Associa os dados da árvore de diálogo ao NPC.
    [Header("Diálogo")]
    [SerializeField] private NPCDialogueData dialogueData;

    [Header("Cartões (CanvasGroup em cada card central)")]
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

        // Separamos o check de áudio dos guardas numa corrotina independente para não lixar o cálculo da visão e pesar no CPU.
        if (npcType == NPCType.Guard)
            StartCoroutine(NoiseCheckRoutine());

        // Se houver uma rota de arranque configurada lançamos logo isso, senão atiramos a máquina de estados para a patrulha geral.
        if (startRoute != null)
            StartCoroutine(RunStartRoute());
        else
            SetState(NPCState.Patrol);
    }

    // Tratamos aqui a prioridade de entregar documentos sobre iniciar a conversa normal mal o jogador interaja com o gajo.
    public override void Interact() {
        if (PlayerController.Instance.heldDocument != null && TaskManager.Instance.HasActiveTask("Entregar documento")) {
            TryDeliverDocument();
            return;
        }

        if (dialogueData == null) {
            Debug.LogWarning($"[NPCScript] {objectName} não tem NPCDialogueData atribuído.");
            return;
        }

        // Paramos a patrulha do NavMeshAgent para a malta não bazar a meio da conversa.
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

        DialogueManager.Instance.OpenDialogue(dialogueData);

        DialogueManager.Instance.OnDialogueClose += ResumeAfterDialogue;
    }

    // Resolve a interação de entregar documentos. Valida pelo ID e espeta logo com o aumento do SuspicionManager se for a pessoa errada.
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

    // Removemos o listener da framework de diálogos para a retoma do estado não duplicar comportamentos.
    private void ResumeAfterDialogue() {
        DialogueManager.Instance.OnDialogueClose -= ResumeAfterDialogue;
        agent.isStopped = false;
        SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
    }

    // Empurra os waypoints iniciais e baralha os destinos. Usamos o WaitForSeconds real para o NPC não ignorar a paragem se as frames quebrarem.
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

        EnterPatrol();
    }

    private void Update() {
        if (currentState == NPCState.Investigate) {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
        }

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

        // Resetar a corrotina é fulcral antes da mudança senão arriscamos os gajos irem para os waypoints enquanto estão num chase state.
        if (patrolCoroutine != null) {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }

        currentState = newState;

        int animState;
        switch (newState) {
            case NPCState.Chase:
                animState = 2;
                break;
            case NPCState.Patrol:
            case NPCState.Investigate:
                animState = 1;
                break;
            default:
                animState = 0;
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
        if (npcType == NPCType.Receptionist && !NPCManager.Instance.CanReceptionistLeave()) {
            SetState(NPCState.Idle);
            return;
        }

        if (assignedRoute != null) {
            currentRoute = assignedRoute;
        } else {
            currentRoute = NPCManager.Instance.GetRandomRoute(npcType, currentFloor, departmentID, currentRoute);

            // Garantimos que não há conflito no canto de repouso recarregando a rota se o sistema achar que o limite já foi atingido.
            if (npcType == NPCType.Guard && currentRoute.isRestRoute && !NPCManager.Instance.CanGuardRest()) {
                currentRoute = NPCManager.Instance.GetRandomRoute(npcType, currentFloor, departmentID, currentRoute, excludeRest: true);
            }
        }

        isResting = currentRoute.isRestRoute;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine() {
        agent.isStopped = false;

        do {
            Transform[] shuffled = (Transform[])currentRoute.waypoints.Clone();
            for (int i = shuffled.Length - 1; i > 0; i--) {
                int rand = Random.Range(0, i + 1);
                Transform temp = shuffled[i];
                shuffled[i] = shuffled[rand];
                shuffled[rand] = temp;
            }

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

        } while (currentRoute.loopWaypoints && currentState == NPCState.Patrol);

        if (currentRoute.returnHome) {
            animator.SetInteger("State", 1);
            agent.SetDestination(homeBase.position);

            while (agent.pathPending || agent.remainingDistance >= 0.5f)
                yield return null;

            animator.SetInteger("State", 0);
        }

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

    // Fazemos um check em ciclos de 100ms para poupar na fatura da renderização e acedemos logo ao trigger do PlayerController para detetar invasões.
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
                float level = GetSuspicionLevelByDistance(dist);

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

    // Calcula os vetores com o campo de visão atual. Expandimos o raio máximo mal a lanterna do jogador ative à noite porque a luz chama muita atenção.
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

    // Escalonamos a suspeita conforme a distância para dar uma hipótese do jogador fugir se estiver nos limites da visão do guarda.
    private float GetSuspicionLevelByDistance(float distance) {
        float third = fovRange / 3f;
        if (distance < third)
            return 2f;

        if (distance < third * 2f)
            return 1.5f;

        return 1f;
    }

    // Corre um intervalo maior que a rotina visual mas só reage se o jogador for barulhento fora dos limites e se o guarda não estiver já a correr para o problema.
    private IEnumerator NoiseCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

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

    // Adapta o estado local baseado nos limites que rebentam no SuspicionManager. Os guardas reagem proativamente na segunda fase.
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

    public void SetPatrolling(bool patrolling) {
        isPatrolling = patrolling;

        if (patrolling && currentState == NPCState.Idle)
            SetState(NPCState.Patrol);
        else if (!patrolling)
            SetState(NPCState.Idle);
    }

    public void ForceRoute(PatrolRoute route) {
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