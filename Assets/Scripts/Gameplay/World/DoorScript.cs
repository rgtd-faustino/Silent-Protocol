using UnityEngine;

public class DoorScript : InteractableObject {

    private bool isOpen = false;

    // referência à fechadura que é filha desta porta
    private LockScript lockScript;

    private void Awake() {
        objectName = "Porta";

        // se não existir, lockScript fica null e a porta abre livremente
        lockScript = GetComponentInChildren<LockScript>();
    }

    public override void Interact() {
        // se não tem fechadura ou a fechadura já foi destravada, abre/fecha
        if (lockScript == null || !lockScript.isLocked) {
            isOpen = !isOpen;
            Debug.Log(isOpen ? "Porta aberta" : "Porta fechada");

        } else {
            Debug.Log("Será que consigo destrancá-la?");
        }
    }
}