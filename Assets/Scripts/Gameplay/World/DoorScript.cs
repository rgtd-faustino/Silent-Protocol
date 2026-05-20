using System.Collections;
using UnityEngine;

public class DoorScript : InteractableObject
{

    [SerializeField] private float anguloAberta = 90f;
    [SerializeField] private float velocidade = 3f;

    private bool isOpen = false;
    private LockScript lockScript;

    [Header("Bloqueio Eletrónico")]
    [SerializeField] private CardReader cardReaderLock;

    // override em vez de private void  garante que o InteractableObject.Awake() inicializa
    // o glitch material, o MeshRenderer e as coordenadas baricntricas corretamente
    protected override void Awake()
    {
        base.Awake();
        objectName = "Porta";
        lockScript = GetComponentInChildren<LockScript>();
        tooltipMessage = "E para abrir/fechar Porta";
    }

    public override void Interact()
    {
        if (lockScript != null && lockScript.isLocked)
        {
            Debug.Log("Ser que consigo destranc-la?");
            return;
        }
        if (cardReaderLock != null && !cardReaderLock.isUnlocked)
        {
            Debug.Log($"[{gameObject.name}] Esta porta está bloqueada eletronicamente. Preciso de usar o leitor de cartões.");
            return;
        }
        Debug.Log("Miau");
        isOpen = !isOpen;
        StopAllCoroutines();
        Quaternion destino = isOpen ? Quaternion.Euler(0f, anguloAberta, 0f) : Quaternion.Euler(0f, 0f, 0f);
        StartCoroutine(AnimarPorta(destino));
    }

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