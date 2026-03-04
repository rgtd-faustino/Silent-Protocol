using UnityEngine;


public class LockScript : InteractableObject {
    private bool isViewOpen = false;
    public int[] code = new int[5];
    private Rigidbody rb;
    public bool isLocked;


    private void Start() {
        rb = GetComponent<Rigidbody>();
    }


    public override void Interact() {
        isViewOpen = !isViewOpen;

        if (isViewOpen) {
            // abrir view do lock
            UIManager.Instance.OpenLockView(this); // passa referência de si próprio
            UIManager.Instance.ChangeCursorState(CursorLockMode.Confined);
            PlayerController.Instance.canMoveRotate = false;

        } else {
            // fechar view do lock
            UIManager.Instance.CloseLockView();
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
        }
    }

    // sincroniza o estado quando a view é fechada pelo jogador em si em vez do código ter sido adivinhado
    public void SyncViewClosed() {
        isViewOpen = false;
    }


    // chamado pelo UIManager quando os 5 dígitos forem introduzidos e retorna true se correto
    public bool TryCode(int[] attempt) {
        for (int i = 0; i < code.Length; i++) {
            if (attempt[i] != code[i]) {
                Debug.Log("Código errado!");
                return false;
            }
        }

        isLocked = false;
        isViewOpen = false;
        gameObject.layer = LayerMask.NameToLayer("Default"); // remove da layer interactable
        enabled = false; // lock resolvido, o script já não é necessário

        Debug.Log("Porta destrancada!");
        return true;
    }
    public void DropLock() {
        rb.useGravity = true; // ativa a gravidade após o delay
    }
}