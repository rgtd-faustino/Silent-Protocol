using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public static PlayerController Instance;

    private float NORMAL_SPEED = 6f;
    private float CROUCH_SPEED = 4f;
    private float RUN_SPEED = 9f;

    // documento fÃ­sico que o jogador estÃ¡ a segurar (apanhado na impressora, para arquivar)
    [HideInInspector] public DocumentTaskData heldDocument = null;
    [HideInInspector] public bool hasFlashlight = false;

    // lista de cartÃµes que o jogador jÃ¡ coletou
    [HideInInspector] public List<string> unlockedCardIDs = new List<string>();

    public void AddCardCredential(string cardID) {
        // sÃ³ adiciona se nÃ£o tiver o cartÃ£o
        if (!unlockedCardIDs.Contains(cardID)) {
            unlockedCardIDs.Add(cardID);
        }
    }

    public bool HasCardCredential(string cardID) {
        return unlockedCardIDs.Contains(cardID);
    }

    // para poder rodar o jogador com o rato
    public Transform cameraTransform;

    // qualquer sistema de UI (lock, PC, cama) mete isto a false quando abre e volta a true quando fecha
    public bool canMoveRotate = true;

    // inSusPlace indica se o jogador estÃ¡ dentro de um trigger marcado como zona suspeita (tag "SusPlace")
    // Ã© lido pelo NPCScript para decidir se gera suspeita ao ver o jogador
    [HideInInspector] public bool inSusPlace = false;

    // raio dentro do qual guardas conseguem ouvir o jogador consoante o tipo de movimento.
    // Crouching -> raio menor; Running -> raio maior.
    // NPCScript consulta GetNoiseRadius() e IsPlayerMoving() para decidir se ouve o jogador.
    [SerializeField] private float normalNoiseRadius = 5f;
    [SerializeField] private float crouchNoiseRadius = 2f;
    [SerializeField] private float runNoiseRadius = 10f;  // correr faz muito mais barulho

    private bool isCrouching = false;
    private bool isRunning = false;   // verdadeiro enquanto Shift + movimento

    private CharacterController cc;
    private CameraScript camScript;
    private Animator animator;

    // o CharacterController nÃ£o aplica fÃ­sica sozinho.
    private float yVelocity = 0f;
    private float gravity = -9.81f;


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    void Start() {
        cc = GetComponent<CharacterController>();
        camScript = cameraTransform.GetComponent<CameraScript>();
        animator = GetComponent<Animator>();

        // subscreve o evento de inÃ­cio de noite para poder reagir
        // (ex: ligar automaticamente a lanterna, mostrar HUD da bateria).
        GameEvent.OnNightStarted += OnNightStarted;

        // Atributo ForÃ§a: Aumenta a velocidade mÃ¡xima de corrida
        if (PlayerStats.Instance != null) {
            RUN_SPEED = 8f + (PlayerStats.Instance.GetForca() * 0.15f);
        }
    }

    void OnDestroy() {
        // desinscrever Ã© obrigatÃ³rio para evitar que o evento tente chamar um mÃ©todo num objeto que jÃ¡ foi destruÃ­do
        GameEvent.OnNightStarted -= OnNightStarted;
    }


    void Update()
    {
        // Tab funciona sempre (fora do canMoveRotate)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EmailUI.AlgumEmailAberto)
                return;
            IntelInventory.Instance.ToggleDossier();
        }

        if (!canMoveRotate)
            return;

        HandleRunning();
        HandleMovement();
        HandleRotation();
        HandleCrouch();
        HandleGravity();

        if (Input.GetKeyDown(KeyCode.B))
            TimeManager.Instance.Coffee();
    }



    // correr só é possível de pé e em movimento — agachado tem prioridade
    private void HandleRunning() {
        bool wasRunning = isRunning;
        isRunning = !isCrouching && Input.GetKey(KeyCode.LeftShift) && IsPlayerMoving();
        
        if (isRunning && !wasRunning) {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_run")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }
        }
    }


    // chamado pelo DocumentPickup quando o jogador interage com o documento
    public void PickupDocument(DocumentTaskData data) {
        heldDocument = data;
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.apanharPapel);
    }


    // GetAxisRaw devolve -1, 0 ou 1 (sem suavizaÃ§Ã£o), o que dÃ¡ resposta imediata e Ã© mais adequado para jogos de aÃ§Ã£o/stealth
    private void HandleMovement() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float speed = isCrouching ? CROUCH_SPEED : (isRunning ? RUN_SPEED : NORMAL_SPEED);
        cc.Move(speed * Time.deltaTime * move);

        // passa os valores ao Animator para que as animaÃ§Ãµes de andar/correr/idle correspondam Ã  direÃ§Ã£o real do movimento
        float animSpeed = isRunning ? 2f : 1f;
        animator.SetFloat("X", x * animSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("Z", z * animSpeed, 0.1f, Time.deltaTime);
    }

    // sÃ³ rotaÃ§Ã£o horizontal porque o eixo vertical (olhar para cima/baixo) Ã© tratado no CameraScript
    private void HandleRotation() {
        float mouseX = Input.GetAxis("Mouse X") * camScript.mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    // a altura e centro do CharacterController mudam com o movimento para que a hitbox corresponda visualmente Ã  postura da personagem
    // os valores sÃ£o diferentes consoante a direÃ§Ã£o do movimento porque a animaÃ§Ã£o de agachamento lateral tem altura diferente da frontal
    private void HandleCrouch() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_crouch")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }
            isCrouching = !isCrouching;
            animator.SetBool("Crouch", isCrouching);
        }

        if (isCrouching) {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            if (x != 0 && z == 0) {
                cc.height = 1.37f; cc.center = new Vector3(0, 0.67f, 0);
            } else if (z != 0) {
                cc.height = 1.43f; cc.center = new Vector3(0, 0.7f, 0);
            } else {
                cc.height = 0.94f; cc.center = new Vector3(0, 0.46f, 0);
            }
        } else {
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.86f, 0);
        }
    }

    private void HandleGravity() {
        yVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
    }

    // chamado quando a noite comeÃ§a (via GameEvent), reservado para HUD da bateria, iluminaÃ§Ã£o, etc.
    private void OnNightStarted() {

    }

    // raio de ruÃ­do atual, consultado pelo NPCScript para saber se o guarda ouve o jogador.
    // correndo -> 10 m  |  normal -> 5 m  |  agachado -> 2 m
    // Atributo Agilidade: Reduz o raio de ruÃ­do gerado.
    public float GetNoiseRadius() {
        float radius = normalNoiseRadius;
        if (isCrouching) radius = crouchNoiseRadius;
        else if (isRunning) radius = runNoiseRadius;

        if (PlayerStats.Instance != null) {
            radius *= (1f - PlayerStats.Instance.GetAgilidade() * 0.05f);
        }
        
        return radius;
    }

    // verdadeiro se o jogador se estiver a mover â€” usado pelo NPCScript para sÃ³ gerar som quando hÃ¡ movimento
    public bool IsPlayerMoving() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        return x != 0 || z != 0;
    }

    // expÃµe o estado de corrida para o HUD ou outros sistemas (ex: stamina futura)
    public bool IsRunning() => isRunning;

    // quando o jogador entra/sai de um collider trigger com tag "SusPlace", atualiza a flag inSusPlace
    // o NPCScript lÃª esta flag para decidir se cria suspeita
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("SusPlace"))
            inSusPlace = true;
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("SusPlace"))
            inSusPlace = false;
    }
}
