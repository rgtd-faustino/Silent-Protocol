using UnityEngine;

public class ImpressoraScript : InteractableObject
{
    public bool documentReady = false;
    public  bool taskActive = false;  // <-- nova flag
    public GameObject documentPickupPrefab;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Impressora";
    }

    protected override bool CheckShouldGlowByDefault()
    {
        return taskActive && !documentReady;
    }

    public void ActivatePrinterTask()
    {
        taskActive = true;
        documentReady = false;
    }

    public override void Interact()
    {
        if (!taskActive)
        {
            Debug.Log("[ImpressoraScript] Esta impressora não tem nenhuma tarefa ativa.");
            return;
        }

        if (!documentReady)
        {
            DocumentPickup pickup = Instantiate(
                documentPickupPrefab,
                transform.position + transform.up * 0.5f,
                Quaternion.identity
            ).GetComponent<DocumentPickup>();
            pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());
            documentReady = true;
            Debug.Log("[ImpressoraScript] Documento impresso. Vai apanhá-lo.");
            return;
        }

        Debug.Log("[ImpressoraScript] O documento já foi impresso.");
    }
}