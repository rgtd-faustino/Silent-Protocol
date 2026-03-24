using UnityEngine;

public class BedScript : InteractableObject {

    // Ao interagir, a cama verifica se é noite, só faz sentido dormir durante a noite. Se for de dia, o jogador recebe feedback
    public override void Interact() {
        if (TimeManager.Instance.isNight) {
            // passa-se a si própria para que o UIManager possa chamar OnSleepConfirmed() quando o sono for confirmado
            // (necessário para futuros efeitos visuais/sonoros específicos da cama)
            UIManager.Instance.OpenSleepView(this);
        } else {
            Debug.Log("Se calhar só posso dormir quando for de noite...");
        }
    }

    public void OnSleepConfirmed(float hours) {
        // reservado para efeitos visuais/sonoros quando existirem
    }
}