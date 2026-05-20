using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public static PlayerController Instance;

    private float NORMAL_SPEED = 6f;
    private float CROUCH_SPEED = 4f;
    private float RUN_SPEED = 9f;

    // documento físico que o jogador está a segurar (apanhado na impressora, para arquivar)
    [HideInInspector] public DocumentTaskData heldDocument = null;
    [HideInInspector] public bool hasFlashlight = false;

    // lista de cartões que o jogador já coletou
    [HideInInspector] public List<string> unlockedCardIDs = new List<string>();

    public void AddCardCredential(string cardID) {
        // só adiciona se não tiver o cartão
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

    // inSusPlace indica se o jogador está dentro de um trigger marcado como zona suspeita (tag "SusPlace")
    // é lido pelo NPCScript para decidir se gera suspeita ao ver o jogador
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

    // o CharacterController não aplica física sozinho.
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

        // subscreve o evento de início de noite para poder reagir
        // (ex: ligar automaticamente a lanterna, mostrar HUD da bateria).
        GameEvent.OnNightStarted += OnNightStarted;
    }

    void OnDestroy() {
        // desinscrever é obrigatório para evitar que o evento tente chamar um método num objeto que já foi destruído
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


    // ---- Running ----

    // correr só é possível de pé e em movimento — agachado tem prioridade
    private void HandleRunning() {
        isRunning = !isCrouching && Input.GetKey(KeyCode.LeftShift) && IsPlayerMoving();
    }


    // chamado pelo DocumentPickup quando o jogador interage com o documento
    public void PickupDocument(DocumentTaskData data) {
        heldDocument = data;

        // mostrar indicador de "tens um documento na mão" no HUD
        // UIManager.Instance.ShowDocumentIndicator(data.documentTitle);
    }


    // GetAxisRaw devolve -1, 0 ou 1 (sem suavização), o que dá resposta imediata e é mais adequado para jogos de ação/stealth
    private void HandleMovement() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float speed = isCrouching ? CROUCH_SPEED : (isRunning ? RUN_SPEED : NORMAL_SPEED);
        cc.Move(speed * Time.deltaTime * move);

        // passa os valores ao Animator para que as animações de andar/correr/idle correspondam à direção real do movimento
        float animSpeed = isRunning ? 1f : 0.5f;
        animator.SetFloat("X", x * animSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("Z", z * animSpeed, 0.1f, Time.deltaTime);
    }

    // só rotação horizontal porque o eixo vertical (olhar para cima/baixo) é tratado no CameraScript
    private void HandleRotation() {
        float mouseX = Input.GetAxis("Mouse X") * camScript.mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    // a altura e centro do CharacterController mudam com o movimento para que a hitbox corresponda visualmente à postura da personagem
    // os valores são diferentes consoante a direção do movimento porque a animação de agachamento lateral tem altura diferente da frontal
    private void HandleCrouch() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
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

    // chamado quando a noite começa (via GameEvent), reservado para HUD da bateria, iluminação, etc.
    private void OnNightStarted() {

    }

    // raio de ruído atual, consultado pelo NPCScript para saber se o guarda ouve o jogador.
    // correndo -> 10 m  |  normal -> 5 m  |  agachado -> 2 m
    // quando PlayerStats existir: radius *= (1f - PlayerStats.Instance.agility * 0.05f);
    public float GetNoiseRadius() {
        if (isCrouching) return crouchNoiseRadius;
        if (isRunning) return runNoiseRadius;
        return normalNoiseRadius;
    }

    // verdadeiro se o jogador se estiver a mover — usado pelo NPCScript para só gerar som quando há movimento
    public bool IsPlayerMoving() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        return x != 0 || z != 0;
    }

    // expõe o estado de corrida para o HUD ou outros sistemas (ex: stamina futura)
    public bool IsRunning() => isRunning;

    // quando o jogador entra/sai de um collider trigger com tag "SusPlace", atualiza a flag inSusPlace
    // o NPCScript lê esta flag para decidir se cria suspeita
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("SusPlace"))
            inSusPlace = true;
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("SusPlace"))
            inSusPlace = false;
    }
}