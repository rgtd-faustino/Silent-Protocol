using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public static PlayerController Instance;

    private float NORMAL_SPEED = 6f;
    private float CROUCH_SPEED = 4f;
    private float RUN_SPEED = 9f;

    [HideInInspector] public DocumentTaskData heldDocument = null;
    [HideInInspector] public bool hasFlashlight = false;

    [HideInInspector] public List<string> unlockedCardIDs = new List<string>();

    public void AddCardCredential(string cardID) {
        if (!unlockedCardIDs.Contains(cardID)) {
            unlockedCardIDs.Add(cardID);
        }
    }

    public bool HasCardCredential(string cardID) {
        return unlockedCardIDs.Contains(cardID);
    }

    public Transform cameraTransform;

    // Colocamos isto a false a partir dos scripts de UI quando abrimos o PC ou tentamos abrir uma fechadura, bloqueando assim o input do jogador. Os mesmos scripts repõem a variável a true no fim.
    public bool canMoveRotate = true;

    // Fica a true quando o jogador pisa num trigger de uma zona restrita. O NPCScript acede diretamente a isto para saber se deve aumentar a barra de suspeita dos guardas.
    [HideInInspector] public bool inSusPlace = false;

    [SerializeField] private float normalNoiseRadius = 5f;
    [SerializeField] private float crouchNoiseRadius = 2f;
    [SerializeField] private float runNoiseRadius = 10f;

    private bool isCrouching = false;
    private bool isRunning = false;

    private CharacterController cc;
    private CameraScript camScript;
    private Animator animator;

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

        // Registamos o evento para saber quando a noite começa e ativar mecânicas específicas, limpamos a subscrição no OnDestroy.
        GameEvent.OnNightStarted += OnNightStarted;

        // Fomos buscar a Força ao PlayerStats para calcularmos logo no início um pequeno bónus na velocidade máxima de corrida.
        if (PlayerStats.Instance != null) {
            RUN_SPEED = 8f + (PlayerStats.Instance.GetForca() * 0.15f);
        }
    }

    void OnDestroy() {
        GameEvent.OnNightStarted -= OnNightStarted;
    }

    void Update()
    {
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

    // Bloqueamos a corrida se o jogador estiver agachado. Comunicamos com o TutorialManager para dar o passo como concluído se estiver na fase certa.
    private void HandleRunning() {
        bool wasRunning = isRunning;
        isRunning = !isCrouching && Input.GetKey(KeyCode.LeftShift) && IsPlayerMoving();
        
        if (isRunning && !wasRunning) {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_run")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }
        }
    }

    public void PickupDocument(DocumentTaskData data) {
        heldDocument = data;
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.apanharPapel);
    }

    // Usámos GetAxisRaw em vez de GetAxis para anularmos a aceleração nativa do Unity. Dá uma sensação muito mais responsiva nos controlos de movimento.
    private void HandleMovement() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float speed = isCrouching ? CROUCH_SPEED : (isRunning ? RUN_SPEED : NORMAL_SPEED);
        cc.Move(speed * Time.deltaTime * move);

        float animSpeed = isRunning ? 2f : 1f;
        animator.SetFloat("X", x * animSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("Z", z * animSpeed, 0.1f, Time.deltaTime);
    }

    // Tratamos apenas da rotação horizontal. O olhar vertical ficou isolado no CameraScript para o modelo do jogador não inclinar.
    private void HandleRotation() {
        float mouseX = Input.GetAxis("Mouse X") * camScript.mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    // Tivemos de adaptar a altura e o centro do collider dependendo da pose da animação para garantir que a hitbox física bate certo com a malha 3D. Os valores foram definidos empiricamente.
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

    private void OnNightStarted() {

    }

    // Fornecemos o raio atual ao NPCScript que o vai consumir a cada frame para calcular a distância do som. Multiplicamos o valor pelo inverso da Agilidade para dar utilidade ao atributo.
    public float GetNoiseRadius() {
        float radius = normalNoiseRadius;
        if (isCrouching) radius = crouchNoiseRadius;
        else if (isRunning) radius = runNoiseRadius;

        if (PlayerStats.Instance != null) {
            radius *= (1f - PlayerStats.Instance.GetAgilidade() * 0.05f);
        }
        
        return radius;
    }

    public bool IsPlayerMoving() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        return x != 0 || z != 0;
    }

    public bool IsRunning() => isRunning;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("SusPlace"))
            inSusPlace = true;
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("SusPlace"))
            inSusPlace = false;
    }
}
