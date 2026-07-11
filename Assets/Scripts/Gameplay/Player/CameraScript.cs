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
        // usamos aqui a Perceção do PlayerStats para dar aos jogadores que apostarem nela uma maior distância de interação com os objetos
        interactionDistance = 5f + (PlayerStats.Instance.GetPercecao() * 0.2f);
        StartCoroutine(DetectInteractableRoutine());
    }

    void Update()
    {
        // se a interface da fechadura estiver aberta e clicarmos na tecla de interação, forçamos o fecho e devolvemos o controlo ao PlayerController
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
        // para o jogador não conseguir rodar a câmara/cabeça mais que este valores/ângulos
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // de modo a interargirmos com um objeto temos de estar a olhar para ele dentro de uma certa distância e temos de premir na tecla de interação
        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            if (TutorialManager.Instance.IsCurrentStepGate("tut_interact")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }

            currentTarget.Interact();
            UIManager.Instance.HideTooltip();
            currentTarget.HideGlitch();
            currentTarget = null;
            currentLock = null;
        }
    }

    // LateUpdate para seguir o headBone só depois da animação já ter atualizado a pose nesta frame de maneira a evitar lag visual
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, headBone.position + Vector3.up * 0.12f, smoothSpeed * Time.deltaTime);
    }

    // optámos por colocar o raycast numa corrotina com um ligeiro atraso em vez do Update. Poupa recursos e para este tipo de interações o jogador não nota a diferença de performance.
    private IEnumerator DetectInteractableRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            DetectInteractable();
            yield return wait;
        }
    }

    // para o jogador conseguir detetar objetos com os quais pode interagir
    // começa por mandar um ray para a frente usando uma esfera, se algo tiver sido apanhado pela mesma que tenha o script interactable object
    // ou que seja filho por hierarquia atribuímos o currentTarget a esse gameObject
    void DetectInteractable() {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.SphereCast(ray, 0.25f, out RaycastHit hit, interactionDistance, interactableLayer)) {
            InteractableObject target = hit.collider.GetComponent<InteractableObject>();

            if (target != null) {
                // só atualizamos o glitch e a tooltip se mudámos de alvo para não estar constantemente
                // a chamar ShowGlitch/ShowTooltip no mesmo objeto

                if (target != currentTarget) {
                    // desliga o glitch do alvo anterior antes de passar para o novo
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

        // se o raycast não apanhou nada interagível, mas ainda tínhamos um alvo guardado de antes,
        // significa que o jogador desviou o olhar portanto limpamos o glitch, a tooltip e as referências
        if (currentTarget != null) {
            UIManager.Instance.HideTooltip();
            currentTarget.HideGlitch();
            currentTarget = null;
            currentLock = null;
        }
    }
}