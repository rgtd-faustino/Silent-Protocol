using UnityEngine;

public class ImpressoraScript : InteractableObject {

    // controla se esta impressora específica pode ser usada agora
    // começa a false e só muda quando o TaskManager chamar ActivatePrinterTask() senão qualquer impressora completaria a task a qualquer momento
    private bool canInteract = false;

    private void Awake() {
        objectName = "Impressora";
    }

    public override void Interact() {
        if (canInteract) {
            TaskManager.Instance.CompleteTask("Imprimir documento", true);
            canInteract = false;

        } else {
            Debug.Log("Ainda não consigo interagir com isto.");
        }
    }

    // chamado pelo TaskManager quando esta impressora é a selecionada para a task
    public void ActivatePrinterTask() {
        canInteract = true;
    }
}