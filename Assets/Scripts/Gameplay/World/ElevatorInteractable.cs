using UnityEngine;

public class ElevatorInteractable : InteractableObject {

    public ElevatorUI elevatorUI;

    protected override void Awake() {
        base.Awake();
        objectName = "painel do elevador";
        tooltipMessage = "E para usar painel do elevador";
    }

    public override void Interact() {
        // se o jogador estiver no menu de jogar ou seja não estiver em nenhum menu então pode usar o elevador
        if (GameMenuManager.Instance.CurrentState == MenuState.Playing)
        {
            // quando o jogador se deslocar ao elevador e o utilizar o tutorial avança para o próximo step
            if (TutorialManager.Instance.IsCurrentStepGate("tut_elevator")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }
            elevatorUI.Open();
        }
    }
}