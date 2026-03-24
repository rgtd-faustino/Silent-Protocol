using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public static PlayerController Instance;

    [SerializeField] private float NORMAL_SPEED = 6f;
    [SerializeField] private float CROUCH_SPEED = 4f;

    // para poder rodar o jogador com o rato
    public Transform cameraTransform;

    // qualquer sistema de UI (lock, PC, cama) mete isto a false quando abre e volta a true quando fecha
    public bool canMoveRotate = true;

    // inSusPlace indica se o jogador está dentro de um trigger marcado como zona suspeita (tag "SusPlace")
    // é lido pelo NPCScript para decidir se gera suspeita ao ver o jogador
    [HideInInspector] public bool inSusPlace = false;

    // o raio de ruído é a distância a que os NPCs conseguem ouvir o jogador
    // agachado produz menos ruído, no futuro, o atributo Agility de PlayerStats reduzirá este valor percentualmente
    // outros scripts (NPCScript) consultam IsPlayerMakingNoise() e GetNoiseRadius() em vez de calcularem por conta própria
    [Header("Ruído")]
    [SerializeField] private float normalNoiseRadius = 5f;
    [SerializeField] private float crouchNoiseRadius = 2f;
    private bool isCrouching = false;

    private CharacterController cc;
    private CameraScript camScript;
    private Animator animator;

    // o CharacterController não aplica física sozinho.
    private float yVelocity = 0f;
    private float gravity = -9.81f;


    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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


    void Update() {
        // bloqueia todo o input de movimento quando uma UI está aberta.
        if (!canMoveRotate) 
            return;

        HandleMovement();
        HandleRotation();
        HandleCrouch();
        HandleGravity();

        // atalho de debug para testar o sistema de café sem precisar de encontrar uma chávena no jogo
        if (Input.GetKeyDown(KeyCode.B))
            TimeManager.Instance.Coffee();
    }


    // GetAxisRaw devolve -1, 0 ou 1 (sem suavização), o que dá resposta imediata e é mais adequado para jogos de ação/stealth
    private void HandleMovement() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        cc.Move((isCrouching ? CROUCH_SPEED : NORMAL_SPEED) * Time.deltaTime * move);

        // passa os valores ao Animator para que as animações de andar/correr/idle correspondam à direção real do movimento
        animator.SetFloat("X", x);
        animator.SetFloat("Z", z);
    }

    // só rotação horizontal porque o eixo vertical (olhar para cima/baixo) é tratado no CameraScript
    private void HandleRotation() {
        float mouseX = Input.GetAxis("Mouse X") * camScript.mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    // a altura e centro do CharacterController mudam com o movimento para que a hitbox  corresponda visualmente à postura da personagem
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

    // consultado pelo NPCScript para saber se deve reagir ao som
    // a lógica de redução por Agility está comentada aqui para quando PlayerStats existir —> o placeholder mantém a estrutura
    public float GetNoiseRadius() {
        float radius = isCrouching ? crouchNoiseRadius : normalNoiseRadius;
        // quando PlayerStats existir: radius *= (1f - PlayerStats.Instance.agility * 0.05f);
        return radius;
    }

    // verdadeiro se o jogador se estiver a mover-se —> usado pelo sistema de ruído para só gerar som quando há movimento
    public bool IsPlayerMoving() {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        return x != 0 || z != 0;
    }


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