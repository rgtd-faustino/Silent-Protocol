using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerController : MonoBehaviour {
    private CharacterController cc;
    [SerializeField] float NORMAL_SPEED = 6f;
    [SerializeField] float CROUCH_SPEED = 4f;
    public Transform cameraTransform;
    private CameraScript camScript;
    private float gravity = -9.81f;
    private float yVelocity = 0f;
    private Animator animator;
    private bool isCrouching;
    public bool canMoveRotate = true;
    private TimeManager tm;

    

    [HideInInspector] public bool inSusPlace = false; // para se o jogador estiver num sitio que não é suposto

    public static PlayerController Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        cc = GetComponent<CharacterController>();
        camScript = cameraTransform.GetComponent<CameraScript>();
        animator = GetComponent<Animator>();
    }

    void Update() {
        if (canMoveRotate == false)
            return;
        

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        cc.Move((isCrouching ? CROUCH_SPEED : NORMAL_SPEED) * Time.deltaTime * move);

        animator.SetFloat("X", x);
        animator.SetFloat("Z", z);

        // para rodar a acamara
        float mouseX = Input.GetAxis("Mouse X") * camScript.mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            isCrouching = !isCrouching;
            animator.SetBool("Crouch", isCrouching);
        }



        // mudar o tamanho do CharacterController consoante a direção e crouch animation
        if (isCrouching) {
            if (x != 0 && z == 0) { // lados
                cc.height = 1.37f;
                cc.center = new Vector3(0, 0.67f, 0);

            } else if (z != 0) { // frente, trás e diagonal
                cc.height = 1.43f;
                cc.center = new Vector3(0, 0.7f, 0);

            } else { // parado
                cc.height = 0.94f;
                cc.center = new Vector3(0, 0.46f, 0);
            }
        } else {
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.86f, 0);
        }

        // adicionar a gravidade
        yVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
    }
    private void Sleep()
    {

    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("SusPlace")) {
            inSusPlace = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("SusPlace")) {
            inSusPlace = false;
        }
    }
}