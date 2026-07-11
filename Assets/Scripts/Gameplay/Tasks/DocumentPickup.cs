using UnityEngine;

public class DocumentPickup : InteractableObject
{
    // o script ImpressoraScript injeta isto através do Initialize, tem a info do departamento e os pesos das escolhas da narrativa
    private DocumentTaskData data;

    // para evitar bugs de duplo clique como apanhar o papel duas vezes seguidas
    private bool isPickedUp = false;

    // o ImpressoraScript chama isto logo após o Instantiate, antes de qualquer update e serve para ligar o modelo físico ao ScriptableObject do dia atual
    public void Initialize(DocumentTaskData documentData)
    {
        data = documentData;
        objectName = $"Documento - {data.documentTitle}";
        tooltipMessage = $"E para apanhar {data.documentTitle}";
    }

    // faz a ponte com o TaskManager para fechar a task de impressão
    // o ato de imprimir já foi processado na PrinterAppUI, portanto aqui apenas sinalizamos que o jogador tem o papel na mão
    public override void Interact()
    {
        if (isPickedUp) 
            return;

        if (PlayerController.Instance.heldDocument != null)
        {
            Debug.Log("[DocumentPickup] Já tens um documento na mão.");
            return;
        }

        if (TutorialManager.Instance.IsCurrentStepGate("tut_task")) {
            TutorialManager.Instance.CompleteCurrentStep();
        }

        isPickedUp = true;
        PlayerController.Instance.PickupDocument(data); // passamos as informações do documento para o jogador
        
        TaskManager.Instance.CompleteTask("Imprimir documento", true);
        gameObject.SetActive(false);
        Debug.Log($"[PlayerController] Apanhei: '{data.documentTitle}' -> departamento {data.correctDepartment}");
    }

    protected override bool CheckShouldGlowByDefault()
    {
        return !isPickedUp;
    }
}
