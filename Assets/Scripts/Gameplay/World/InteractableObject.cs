using UnityEngine;

public class InteractableObject : MonoBehaviour {

    [HideInInspector] public string objectName = "objeto";


    private void Start() {

    }

    private void Update() {
        
    }


    public virtual void Interact() {
        Debug.Log($"{objectName} sofreu interação");
    }
}