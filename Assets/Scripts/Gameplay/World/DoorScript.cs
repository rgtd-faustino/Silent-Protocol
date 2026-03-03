using UnityEditor;
using UnityEngine;

public class DoorScript : InteractableObject {

    private bool isOpen = false;





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
        if (transform.GetComponentInChildren<LockScript>().isLocked) {
            Debug.Log("Será que consigo destrancá-la?");

        } else {
            isOpen = !isOpen;
            Debug.Log(isOpen ? "Porta aberta" : "Porta fechada");
        }
    }
}
