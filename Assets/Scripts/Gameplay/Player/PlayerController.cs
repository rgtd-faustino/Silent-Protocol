using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerController : MonoBehaviour {
    private CharacterController cc;
    [SerializeField] float speed = 6;
    public Transform cameraTransform;
    private CameraScript camScript;
    private float originalHeight;
    private float gravity = -9.81f;
    private float yVelocity = 0f;
    private Animator animator;
    private bool isCrouching;

    void Start() {
        cc = GetComponent<CharacterController>();
        camScript = cameraTransform.GetComponent<CameraScript>();
        originalHeight = cc.height;
        animator = GetComponent<Animator>();
    }

    void Update() {
        // Movimento
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        cc.Move(speed * Time.deltaTime * move);

        // Animação
        animator.SetFloat("X", x);
        animator.SetFloat("Z", z);

        // Rotação horizontal
        float mouseX = Input.GetAxis("Mouse X") * camScript.mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            isCrouching = !isCrouching;
            animator.SetBool("Crouch", isCrouching);
        }

        // Ajuste do CharacterController consoante direção e crouch
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

        // Gravidade
        yVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
    }
}