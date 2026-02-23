using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked; // esconde o rato
    }

    void Update() {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // limita para não virar de mais
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
