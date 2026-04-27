using UnityEngine;

public class ElevatorInteractable : InteractableObject {

    public ElevatorUI elevatorUI;

    void Start() {
        objectName = "painel do elevador";
    }

    public override void Interact() {
        if (GameMenuManager.Instance.CurrentState == MenuState.Playing)
            elevatorUI.Open();
    }
}