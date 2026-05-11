using UnityEngine;

public class PCInteractable : InteractableObject {

    public GameObject pcCanvas;

    private bool isOpen = false;

    public override void Interact() {
        if (!isOpen)
            OpenPC();
        else
            ClosePC();
    }

    private void OpenPC()
    {
        pcCanvas.SetActive(true);

        PlayerController.Instance.canMoveRotate = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UIManager.Instance.RefreshPCInterface();

        isOpen = true;

        // reset ao estado do servidor se existir
        Server1UI srv1 = pcCanvas.GetComponentInChildren<Server1UI>();
        if (srv1 != null) srv1.OnOpen();

        Server2UI srv2 = pcCanvas.GetComponentInChildren<Server2UI>();
        if (srv2 != null) srv2.OnOpen();
    }

    private void Update() {
        // só tenta apanahr o Escape se o PC estiver aberto
        if (isOpen && Input.GetKeyDown(KeyCode.P))
            ClosePC();
    }

    private void ClosePC()
    {
        pcCanvas.SetActive(false);

        PlayerController.Instance.canMoveRotate = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isOpen = false;
    }
}