using UnityEngine;

public class BedScript : InteractableObject {

    protected override void Awake() {
        base.Awake();
        objectName = "Cama";
        tooltipMessage = "E para deitar";
    }

    // Ao interagir, a cama verifica se  noite, s faz sentido dormir durante a noite. Se for de dia, o jogador recebe feedback
    public override void Interact() {
        if (TimeManager.Instance.isNight) {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_sleep")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }

            // passa-se a si prpria para que o UIManager possa chamar OnSleepConfirmed() quando o sono for confirmado
            // (necessrio para futuros efeitos visuais/sonoros especficos da cama)
            UIManager.Instance.OpenSleepView(this);
        } else {
            Debug.Log("Se calhar s posso dormir quando for de noite...");
        }
    }

    public void OnSleepConfirmed(float hours) {
        // reservado para efeitos visuais/sonoros quando existirem
    }
}