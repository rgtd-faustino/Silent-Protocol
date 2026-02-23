using UnityEngine;

public class PlayerController : MonoBehaviour {
    private CharacterController cc;
    [SerializeField] float speed = 6;
    public Transform cameraTransform;
    private int crouchDistance = 4;
    private CameraScript camScript;
    private float originalHeight;
    private float gravity = -9.81f;
    private float yVelocity = 0f;
    private Animator animator;

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
            cc.height = originalHeight / 2;
            cameraTransform.localPosition = new Vector3(0, crouchDistance / 2, 0);
            animator.SetBool("Crouch", true);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            cc.height = originalHeight;
            cameraTransform.localPosition = new Vector3(0, crouchDistance, 0);
            animator.SetBool("Crouch", false);
        }

        // Gravidade
        yVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
    }
}