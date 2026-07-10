using UnityEngine;

public class DocumentPickup : InteractableObject
{

    // referncia ao ScriptableObject do documento > passada pelo ImpressoraScript no Instantiate
    //  o que nos diz para que departamento deve ir e quais os pesos narrativos
    private DocumentTaskData data;

    // flag que impede que o jogador apanhe o documento se j tiver um na mo (cada dia s h um documento para arquivar)
    private bool isPickedUp = false;


    // chamado pela ImpressoraScript imediatamente aps o Instantiate
    public void Initialize(DocumentTaskData documentData)
    {
        data = documentData;
        objectName = $"Documento - {data.documentTitle}";
        tooltipMessage = $"E para apanhar {data.documentTitle}";
    }


    public override void Interact()
    {
        if (isPickedUp) return;
        if (PlayerController.Instance.heldDocument != null)
        {
            Debug.Log("[DocumentPickup] J tens um documento na mo.");
            return;
        }

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_task")) {
            TutorialManager.Instance.CompleteCurrentStep();
        }

        isPickedUp = true;
        PlayerController.Instance.PickupDocument(data);
        TaskManager.Instance.CompleteTask("Imprimir documento", true);
        gameObject.SetActive(false);
        Debug.Log($"[PlayerController] Apanhei: '{data.documentTitle}' -> departamento {data.correctDepartment}");
    }

    protected override bool CheckShouldGlowByDefault()
    {
        return !isPickedUp;
    }
}
