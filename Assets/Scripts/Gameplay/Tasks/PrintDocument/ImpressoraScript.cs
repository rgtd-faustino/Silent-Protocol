using UnityEngine;

public class ImpressoraScript : InteractableObject
{

    private bool documentReady = false;
    public GameObject documentPickupPrefab;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Impressora";
    }

    protected override bool CheckShouldGlowByDefault()
    {
        return documentReady;
    }

    // chamado pelo TaskManager quando esta impressora é a selecionada
    // spawna o documento imediatamente — o jogador só precisa de ir lá apanhá-lo
    public void ActivatePrinterTask()
    {
        DocumentPickup pickup = Instantiate(
            documentPickupPrefab,
            transform.position + Vector3.up * 0.1f,
            Quaternion.identity,
            transform
        ).GetComponent<DocumentPickup>();

        pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());

        // regista que há um documento à espera nesta impressora
        documentReady = true;
    }

    // chamado quando o jogador interage com a impressora DEPOIS de o documento já ter sido spawnado
    // a task completa-se aqui, quando o jogador apanha o documento
    public override void Interact()
    {
        if (!documentReady)
        {
            // Ainda não há documento — spawna agora
            DocumentPickup pickup = Instantiate(
                documentPickupPrefab,
                transform.position + transform.forward * 1.5f + Vector3.up * 0.1f,
                Quaternion.identity
            ).GetComponent<DocumentPickup>();
            pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());
            documentReady = true;
            Debug.Log("[ImpressoraScript] Documento impresso. Vai apanhá-lo.");
            return;
        }

        // Já foi spawnado anteriormente (edge case)
        Debug.Log("[ImpressoraScript] O documento já foi impresso.");
    }
}