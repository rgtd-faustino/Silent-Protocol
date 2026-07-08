using System.Collections;
using UnityEngine;

public class DoorScript : InteractableObject
{

    private AudioSource audioSource;

    [SerializeField] private float anguloAberta = 90f;
    [SerializeField] private float velocidade = 3f;

    private bool isOpen = false;

    [Header("Fechadura de Código")]
    [SerializeField] private LockScript lockScript;

    [Header("Bloqueio Eletrónico")]
    [SerializeField] private CardReader cardReaderLock;

    // override em vez de private void  garante que o InteractableObject.Awake() inicializa
    // o glitch material, o MeshRenderer e as coordenadas baricntricas corretamente
    protected override void Awake()
    {
        base.Awake();
        objectName = "Porta";
        tooltipMessage = "E para abrir/fechar Porta";

        if (lockScript != null && cardReaderLock != null)
        {
            Debug.LogError($"[{gameObject.name}] Esta porta tem LockScript E CardReader atribuídos. Uma porta só pode ter um dos dois. Remove um deles.");
        }
    }

    private void Start() {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // aviso visível no Inspector mesmo sem correr o jogo, para apanhar o erro em modo de edição
    private void OnValidate()
    {
        if (lockScript != null && cardReaderLock != null)
        {
            Debug.LogWarning($"[{gameObject.name}] ATENÇÃO: porta configurada com LockScript e CardReader em simultâneo. Escolhe apenas um.");
        }
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

        isOpen = !isOpen;
        AudioClip clip = isOpen ? SoundManager.Instance.openDoor : SoundManager.Instance.closeDoor;
        SoundManager.Instance.PlayDoorSound(this.gameObject, clip);

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