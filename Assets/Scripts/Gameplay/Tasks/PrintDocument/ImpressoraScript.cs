using UnityEngine;

public class ImpressoraScript : InteractableObject
{

    private bool documentReady = false;
    public GameObject documentPickupPrefab;

    private void Awake()
    {
        objectName = "Impressora";
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
            Debug.Log("[ImpressoraScript] Não há nenhum documento para apanhar.");
            return;
        }

        TaskManager.Instance.CompleteTask("Imprimir documento", true);
        documentReady = false;
        Debug.Log("[ImpressoraScript] Documento apanhado. Task completa.");
    }
}