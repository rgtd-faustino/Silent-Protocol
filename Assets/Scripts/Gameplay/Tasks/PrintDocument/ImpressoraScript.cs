using UnityEngine;

public class ImpressoraScript : InteractableObject
{
    private bool documentReady = false;
    private bool taskActive = false;
    public GameObject documentPickupPrefab;
    public GameObject placeholder;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Impressora";
        tooltipMessage = "E para usar impressora";
        
        placeholder = transform.Find("Cube").gameObject;
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
            Debug.Log("[ImpressoraScript] Esta impressora no tem nenhuma tarefa ativa.");
            return;
        }

        if (!documentReady)
        {
            DocumentPickup pickup = Instantiate(
                documentPickupPrefab,
                placeholder.transform.position,Quaternion.identity
            ).GetComponent<DocumentPickup>();
            pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());
            documentReady = true;
            Debug.Log("[ImpressoraScript] Documento impresso e ejetado.");
            return;
        }

        Debug.Log("[ImpressoraScript] O documento j foi impresso.");
    }
}