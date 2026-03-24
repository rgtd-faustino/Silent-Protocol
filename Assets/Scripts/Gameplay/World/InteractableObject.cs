using UnityEngine;

public class InteractableObject : MonoBehaviour {

    // nome do objeto para mensagens de debug e para o tooltip que aparece no ecrã quando o jogador aponta para o objeto
    [HideInInspector] public string objectName = "objeto";

    // chamado se uma subclasse não fizer override
    public virtual void Interact() {
        Debug.Log($"{objectName} sofreu interação");
    }
}