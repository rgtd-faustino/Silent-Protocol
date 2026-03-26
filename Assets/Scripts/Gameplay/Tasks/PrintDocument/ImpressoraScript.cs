using UnityEngine;

public class ImpressoraScript : InteractableObject {

    // controla se esta impressora específica pode ser usada agora
    // começa a false e só muda quando o TaskManager chamar ActivatePrinterTask() senão qualquer impressora completaria a task a qualquer momento
    private bool canInteract = false;
    public GameObject documentPickupPrefab;



    private void Awake() {
        objectName = "Impressora";
    }

    public override void Interact() {
        if (canInteract) {
            TaskManager.Instance.CompleteTask("Imprimir documento", true);

            // instancia o documento físico em cima da impressora
            // e inicia-o com os dados do documento do dia para que o ArchiveScript saiba para onde deve ir
            DocumentPickup pickup = Instantiate(documentPickupPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity, transform).GetComponent<DocumentPickup>();
            pickup.Initialize(DocumentManager.Instance.GetDocumentForToday());
            canInteract = false;

        } else {
            Debug.Log("[ImpressoraScript] Ainda não consigo interagir com isto.");
        }
    }

    // chamado pelo TaskManager quando esta impressora é a selecionada para a task
    public void ActivatePrinterTask() {
        canInteract = true;
    }
}