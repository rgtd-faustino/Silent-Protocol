using UnityEngine;

public class DocumentPickup : InteractableObject
{

    // referência ao ScriptableObject do documento —> passada pelo ImpressoraScript no Instantiate
    // é o que nos diz para que departamento deve ir e quais os pesos narrativos
    private DocumentTaskData data;

    // flag que impede que o jogador apanhe o documento se já tiver um na mão (cada dia só há um documento para arquivar)
    private bool isPickedUp = false;


    // chamado pela ImpressoraScript imediatamente após o Instantiate
    public void Initialize(DocumentTaskData documentData)
    {
        data = documentData;
        objectName = $"Documento — {data.documentTitle}";
    }


    public override void Interact()
    {
        if (isPickedUp) return;
        if (PlayerController.Instance.heldDocument != null)
        {
            Debug.Log("[DocumentPickup] Já tens um documento na mão.");
            return;
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