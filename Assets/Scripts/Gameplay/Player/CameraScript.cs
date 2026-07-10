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
    [HideInInspector] public bool blockDetection = false;

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
        // Puxámos a Perceção do PlayerStats para dar aos jogadores que apostarem nela uma maior distância de interação com os objetos.
        if (PlayerStats.Instance != null) {
            interactionDistance = 5f + (PlayerStats.Instance.GetPercecao() * 0.2f);
        }

        StartCoroutine(DetectInteractableRoutine());
    }

    void Update()
    {
        // Se a interface da fechadura estiver aberta e clicarmos na tecla de interação, forçamos o fecho e devolvemos o controlo ao PlayerController.
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
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_interact")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }

            currentTarget.Interact();
            UIManager.Instance.HideTooltip();
            currentTarget.HideGlitch();
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

    // Optámos por colocar o raycast numa corrotina com um ligeiro atraso em vez do Update. Poupa recursos e para este tipo de interações o jogador não nota a diferença de performance.
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
        if (blockDetection) return;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.SphereCast(ray, 0.25f, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            InteractableObject target = hit.collider.GetComponent<InteractableObject>();
            if (target != null)
            {
                if (target != currentTarget)
                {
                    if (currentTarget != null)
                        currentTarget.HideGlitch();

                    currentTarget = target;
                    currentTarget.ShowGlitch();
                    currentLock = target as LockScript;

                    UIManager.Instance.ShowTooltip(currentTarget.tooltipMessage);
                }
                return;
            }
        }

        if (currentTarget != null)
        {
            UIManager.Instance.HideTooltip();
            currentTarget.HideGlitch();
            currentTarget = null;
            currentLock = null;
        }
    }
}