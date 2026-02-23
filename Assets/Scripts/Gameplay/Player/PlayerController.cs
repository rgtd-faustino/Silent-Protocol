using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController cc;
    [SerializeField] float speed = 6;
    public Transform cameraTransform;
    private int crouchDistance = 4;
    private CameraScript camScript;
    private float originalHeight;
    private float gravity = -9.81f;
    private float yVelocity = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        camScript = cameraTransform.GetComponent<CameraScript>();
        originalHeight = cc.height;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            cc.height = originalHeight / 2;
            cameraTransform.localPosition = new Vector3(0, crouchDistance, 0);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            cc.height = originalHeight;
            cameraTransform.localPosition = new Vector3(0, crouchDistance, 0);
        }

        yVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
    }

}
