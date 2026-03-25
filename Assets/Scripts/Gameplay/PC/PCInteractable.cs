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

    private void OpenPC() {
        pcCanvas.SetActive(true);

        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None); // cursor livre para clicar na UI

        // atualiza os botões do PC (ex: impressão) com base no estado atual das tasks —> feito aqui porque o canvas acaba de ficar visível
        UIManager.Instance.RefreshPCInterface();

        isOpen = true;
    }

    private void Update() {
        // só tenta apanahr o Escape se o PC estiver aberto
        if (isOpen && Input.GetKeyDown(KeyCode.P))
            ClosePC();
    }

    private void ClosePC() {
        pcCanvas.SetActive(false);

        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked); // cursor volta a estar escondido

        isOpen = false;
    }
}