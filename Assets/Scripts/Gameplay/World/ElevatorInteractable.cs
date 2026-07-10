using UnityEngine;

public class ElevatorInteractable : InteractableObject {

    public ElevatorUI elevatorUI;

    protected override void Awake() {
        base.Awake();
        objectName = "painel do elevador";
        tooltipMessage = "E para usar painel do elevador";
    }

    // antes de abrir o UI do elevador validamos o estado do menu para garantir que o jogador nao consegue
    // usar o elevador durante uma cutscene ou dialogo. o gate do tutorial avanca o passo quando o jogador interage pela primeira vez
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