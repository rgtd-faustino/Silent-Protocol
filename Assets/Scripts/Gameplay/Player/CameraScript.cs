using System.Collections;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance;

    [Header("Mouse")]
    public float mouseSensitivity = 250f;
    private float xRotation = 0f;

    [Header("Head Tracking")]
    public Transform headBone;
    public float smoothSpeed = 20f;

    [Header("Interaction")]
    public float interactionDistance = 6f;
    public LayerMask interactableLayer;
    private KeyCode interactKey = KeyCode.E;
    [HideInInspector] public InteractableObject currentTarget;
    private LockScript currentLock;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    void Start()
    {
        StartCoroutine(DetectInteractableRoutine());
    }

    void Update()
    {
        if (UIManager.Instance.IsLockViewOpen() && Input.GetKeyDown(interactKey))
        {
            if (currentLock != null)
                currentLock.SyncViewClosed();

            UIManager.Instance.CloseLockView();
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
            return;
        }

        if (PlayerController.Instance.canMoveRotate == false)
            return;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.Interact();
            UIManager.Instance.HideTooltip();
            currentTarget.HideGlitch(); // esconde antes de limpar a referência
            currentTarget = null;
            currentLock = null;
        }
    }

    void LateUpdate()
    {
        if (headBone == null) return;
        transform.position = Vector3.Lerp(
            transform.position,
            headBone.position + Vector3.up * 0.12f,
            smoothSpeed * Time.deltaTime
        );
    }

    private IEnumerator DetectInteractableRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            DetectInteractable();
            yield return wait;
        }
    }

    void DetectInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.SphereCast(ray, 0.25f, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            InteractableObject target = hit.collider.GetComponent<InteractableObject>();
            if (target != null)
            {
                if (target != currentTarget)
                {
                    // esconde o anterior antes de mudar
                    if (currentTarget != null)
                        currentTarget.HideGlitch();

                    currentTarget = target;
                    currentTarget.ShowGlitch();
                    currentLock = target as LockScript;

                    if (target is IntelPickup)
                        UIManager.Instance.ShowTooltip("E para ler  |  G para guardar");
                    else
                        UIManager.Instance.ShowTooltip("E para interagir");
                }
                return;
            }
        }

        // perdeu o target
        if (currentTarget != null)
        {
            UIManager.Instance.HideTooltip();
            currentTarget.HideGlitch(); // primeiro
            currentTarget = null;       // depois
            currentLock = null;
        }
    }
}