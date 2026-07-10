using UnityEngine;

public class ImpressoraScript : InteractableObject
{
    // Estas duas variáveis operam de forma independente para gerir o estado. A task pode ser ativada na UI da impressora, mas o papel só fica pronto quando o jogador interage fisicamente com a máquina.
    private bool documentReady = false;
    private bool taskActive = false;

    public GameObject documentPickupPrefab;

    // Referência para o objeto filho onde o prefab do documento vai ser instanciado no mundo.
    public GameObject placeholder;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Impressora";
        tooltipMessage = "E para usar impressora";
        
        placeholder = transform.Find("Cube").gameObject;
    }

    // O brilho ajuda a orientar o jogador. Assim que o documento sai, a impressora apaga-se e o brilho passa para o documento em si.
    protected override bool CheckShouldGlowByDefault()
    {
        return taskActive && !documentReady;
    }

    // O TaskManager chama este método quando uma tarefa de impressão é validada na PrinterAppUI. O sistema escolhe uma impressora à sorte, obrigando o jogador a andar pelo mapa.
    public void ActivatePrinterTask()
    {
        taskActive = true;
        documentReady = false;
    }

    // Cria o prefab do documento no placeholder e injeta os dados do DocumentManager. A partir deste ponto, o script DocumentPickup toma conta do resto da interação.
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
                placeholder.transform.position,Quaternion.identity
            ).GetComponent<DocumentPickup>();
            pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());
            documentReady = true;
            Debug.Log("[ImpressoraScript] Documento impresso e ejetado.");
            return;
        }

        Debug.Log("[ImpressoraScript] O documento já foi impresso.");
    }
}