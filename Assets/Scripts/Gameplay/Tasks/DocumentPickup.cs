using UnityEngine;

public class DocumentPickup : InteractableObject
{
    // O ImpressoraScript injeta isto através do Initialize. Tem a info do departamento e os pesos das escolhas da narrativa.
    private DocumentTaskData data;

    // Uma flag simples para evitar bugs de duplo clique, garantindo que não apanhamos o papel duas vezes seguidas.
    private bool isPickedUp = false;

    // O ImpressoraScript chama isto logo após o Instantiate, antes de qualquer update. Serve para ligar o modelo físico ao ScriptableObject do dia atual.
    public void Initialize(DocumentTaskData documentData)
    {
        data = documentData;
        objectName = $"Documento - {data.documentTitle}";
        tooltipMessage = $"E para apanhar {data.documentTitle}";
    }

    // Faz a ponte com o TaskManager para fechar a task de impressão. O ato de imprimir já foi processado na PrinterAppUI, portanto aqui apenas sinalizamos que o jogador tem o papel na mão.
    public override void Interact()
    {
        if (isPickedUp) return;
        if (PlayerController.Instance.heldDocument != null)
        {
            Debug.Log("[DocumentPickup] Já tens um documento na mão.");
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
