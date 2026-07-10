using UnityEngine;

public class BedScript : InteractableObject {

    protected override void Awake() {
        base.Awake();
        objectName = "Cama";
        tooltipMessage = "E para deitar";
    }

    // a cama vai ler a propriedade isNight do TimeManager. faz todo o sentido barrar o sono se for de dia para forçar o jogador a lidar com as horas normais
    public override void Interact() {
        if (TimeManager.Instance.isNight) {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_sleep")) {
                TutorialManager.Instance.CompleteCurrentStep();
            }

            // atiramos o objeto em si para o UIManager conseguir avisar o callback do OnSleepConfirmed caso o utilizador aceite o prompt no menu
            UIManager.Instance.OpenSleepView(this);
        } else {
            Debug.Log("Se calhar só posso dormir quando for de noite...");
        }
    }

    public void OnSleepConfirmed(float hours) {
    }
}