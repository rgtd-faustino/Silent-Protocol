using UnityEngine;

public class BedScript : InteractableObject {

    protected override void Awake() {
        base.Awake();
        objectName = "Cama";
        tooltipMessage = "E para dormir";
    }

    // a cama vai ler a propriedade isNight do TimeManager
    // não é permitido dormir durante o dia, apenas durante a noite
    public override void Interact() {
        if (TimeManager.Instance.isNight) {
            // mostramos o step do tutorial para dizer ao jogador que tem de dormir
            if (TutorialManager.Instance.IsCurrentStepGate("tut_sleep")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }

            // mandamos o objeto em si para o UIManager conseguir avisar o callback do OnSleepConfirmed caso o utilizador aceite o prompt no menu
            UIManager.Instance.OpenSleepView(this);
        } else {
            Debug.Log("Se calhar só posso dormir quando for de noite...");
        }
    }
}