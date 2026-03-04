using UnityEngine;

public class CameraScript : MonoBehaviour {
    [Header("Mouse")]
    public float mouseSensitivity = 250f;
    private float xRotation = 0f;

    [Header("Head Tracking")]
    public Transform headBone;
    public float smoothSpeed = 20f; // 0 = sem smooth, valores altos = mais responsivo

    [Header("Interaction")]
    public float interactionDistance = 6f;
    public LayerMask interactableLayer;
    private KeyCode interactKey = KeyCode.E;
    private InteractableObject currentTarget;
    private LockScript currentLock;


    void Start() {
    }

    void Update() {
        // se a lock view está aberta e o jogador clica no E então fecha a view
        if (UIManager.Instance.IsLockViewOpen() && Input.GetKeyDown(interactKey)) {
            // se for o cadeado então damos sync
            if (currentLock != null) {
                currentLock.SyncViewClosed();
            }

            UIManager.Instance.CloseLockView();
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
            return;
        }

        if (PlayerController.Instance.canMoveRotate == false)
            return;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // limita para não virar demasiado
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        DetectInteractable();

        if (currentTarget != null && Input.GetKeyDown(interactKey))
            currentTarget.Interact();
    }

    // late update porque é a última frame a ser executada e é por isso que a usamos na câmara
    void LateUpdate() {
        if (headBone == null) return;

        // segue a posição do osso da cabeça após as animações serem calculadas
        // smooth para não tremer
        // subimos 0.12 para a câmara ficar ao nível dos olhos
        transform.position = Vector3.Lerp(transform.position, headBone.position + Vector3.up * 0.12f, smoothSpeed * Time.deltaTime);
    }

    void DetectInteractable() {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer)) {
            InteractableObject target = hit.collider.GetComponent<InteractableObject>();
            if (target != null) {
                if (target != currentTarget) { // só atualiza se mudou
                    currentTarget = target;
                    currentLock = target as LockScript; // guardamos a referencia no current lock se for o target for um cadeado para depois podermos dar sync se o jogador quiser sair da view
                    UIManager.Instance.ShowTooltip();
                }
                return;
            }
        }

        if (currentTarget != null) { // só esconde se havia algo antes
            UIManager.Instance.HideTooltip();
            currentTarget = null;
            currentLock = null;
        }
    }
}