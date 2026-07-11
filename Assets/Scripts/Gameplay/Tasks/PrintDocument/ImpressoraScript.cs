using UnityEngine;

public class ImpressoraScript : InteractableObject
{
    // estas duas variáveis operam de forma independente para gerir o estado
    // a task pode ser ativada na UI da impressora, mas o papel só fica pronto quando o jogador interage fisicamente com a impressora
    private bool documentReady = false;
    private bool taskActive = false;

    public GameObject documentPickupPrefab;

    // referência para o objeto filho onde o prefab do documento vai ser instanciado no mundo
    public GameObject placeholder;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Impressora";
        tooltipMessage = "E para imprimir documento";
        placeholder = transform.Find("Cube").gameObject;
    }

    // o efeito de glitch ajuda a orientar o jogador, assim que o documento sai oo brilho da impressora apaga-se e passa para o documento em si
    protected override bool CheckShouldGlowByDefault()
    {
        return taskActive && !documentReady;
    }

    // o TaskManager chama este método quando uma tarefa de impressão é validada na PrinterAppUI
    // escolhemos uma impressora à sorte de modo a obrigarmos o jogador a andar pelo mapa
    public void ActivatePrinterTask()
    {
        taskActive = true;
        documentReady = false;
    }

    // cria o prefab do documento no placeholder e injeta os dados do DocumentManager
    public override void Interact()
    {
        if (!taskActive)
        {
            Debug.Log("[ImpressoraScript] Esta impressora não tem nenhuma tarefa ativa.");
            return;
        }

        if (!documentReady)
        {
            DocumentPickup pickup = Instantiate(documentPickupPrefab, placeholder.transform.position,Quaternion.identity).GetComponent<DocumentPickup>();
            pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());
            documentReady = true;
            Debug.Log("[ImpressoraScript] Documento impresso e ejetado.");
            return;
        }

        Debug.Log("[ImpressoraScript] O documento já foi impresso.");
    }
}