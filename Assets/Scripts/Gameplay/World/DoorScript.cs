using System.Collections;
using UnityEngine;

public class DoorScript : InteractableObject {

    [SerializeField] private float anguloAberta = 90f;
    [SerializeField] private float velocidade = 3f;

    private bool isOpen = false;
    private LockScript lockScript;

    private void Awake() {
        objectName = "Porta";
        lockScript = GetComponentInChildren<LockScript>();
    }

    public override void Interact() {
        if (lockScript != null && lockScript.isLocked) {
            Debug.Log("Será que consigo destrancá-la?");
            return;
        }

        isOpen = !isOpen;
        StopAllCoroutines();
        Quaternion destino = isOpen ? Quaternion.Euler(0f, anguloAberta, 0f) : Quaternion.Euler(0f, 0f, 0f);
        StartCoroutine(AnimarPorta(destino));
    }

    private IEnumerator AnimarPorta(Quaternion destino) {
        while (Quaternion.Angle(transform.localRotation, destino) > 0.1f) {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, destino, Time.deltaTime * velocidade
            );
            yield return null;
        }
        transform.localRotation = destino;
    }
}