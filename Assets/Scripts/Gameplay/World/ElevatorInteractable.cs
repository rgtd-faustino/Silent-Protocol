using UnityEngine;

public class ElevatorInteractable : InteractableObject {

    public ElevatorUI elevatorUI;

    protected override void Awake() {
        base.Awake();
        objectName = "painel do elevador";
        tooltipMessage = "E para usar painel do elevador";
    }

    public override void Interact() {
        if (GameMenuManager.Instance.CurrentState == MenuState.Playing)
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_elevator")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }
            elevatorUI.Open();
        }
    }
}