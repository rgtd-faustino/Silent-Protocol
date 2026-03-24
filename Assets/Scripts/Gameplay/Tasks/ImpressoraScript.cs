using UnityEditor;
using UnityEngine;

public class ImpressoraScript : InteractableObject
{






    private void Awake() {
        objectName = "Porta";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }




    public override void Interact() {
        TaskManager.Instance.CompleteTask("Imprimir documento", true);
    }
}
