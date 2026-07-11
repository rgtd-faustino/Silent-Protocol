using System.Collections;
using UnityEngine;

public class DoorScript : InteractableObject
{

    [SerializeField] private float anguloAberta = 90f;
    [SerializeField] private float velocidade = 3f;

    private Coroutine animCoroutine; // corrotina que corre a nimação de abrir ou fechar a porta
    private bool isOpen = false;

    [Header("Fechadura de Código")]
    [SerializeField] private LockScript lockScript;

    [Header("Bloqueio Eletrónico")]
    [SerializeField] private CardReader cardReaderLock;

    // override em vez de private void garante que o InteractableObject.Awake() inicializa
    // o glitch material, o MeshRenderer e as coordenadas baricêntricas corretamente
    protected override void Awake()
    {
        base.Awake();
        objectName = "Porta";

        // fazemos uma corrotina para não usarmos um update que seria muito ineficiente
        StartCoroutine(TooltipUpdateRoutine());

        if (lockScript != null && cardReaderLock != null)
        {
            Debug.LogError($"[{gameObject.name}] Esta porta tem LockScript E CardReader atribuídos. Uma porta só pode ter um dos dois. Remove um deles.");
        }
    }

    private IEnumerator TooltipUpdateRoutine() {
        while (true) {
            UpdateTooltip();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateTooltip() {
        if (isOpen) {
            tooltipMessage = "E - Fechar porta";
        } else {
            tooltipMessage = "E - Abrir porta";
        }
    }

    public override void Interact() {
        if (lockScript != null && lockScript.isLocked) {
            Debug.Log("Esta porta está bloqueada com um código. Preciso do descobrir!");
            return;
        }

        if (cardReaderLock != null && !cardReaderLock.isUnlocked) {
            Debug.Log($"Esta porta está bloqueada com um cartão. Preciso do encontrar!");
            return;
        }

        isOpen = !isOpen;
        SoundManager.Instance.PlayDoorSound(gameObject, isOpen);

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        Quaternion destino = isOpen ? Quaternion.Euler(0f, anguloAberta, 0f) : Quaternion.Euler(0f, 0f, 0f);
        animCoroutine = StartCoroutine(AnimarPorta(destino));
    }

    // animação de abrir ou fechar a porta
    private IEnumerator AnimarPorta(Quaternion destino)
    {
        while (Quaternion.Angle(transform.localRotation, destino) > 0.1f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, destino, Time.deltaTime * velocidade);
            yield return null;
        }
        transform.localRotation = destino;
    }
}