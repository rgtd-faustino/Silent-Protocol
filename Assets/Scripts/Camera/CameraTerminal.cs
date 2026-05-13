using UnityEngine;

public class CameraTerminal : InteractableObject {
    [SerializeField] private bool nightOnly = false;

    void Start() {
        objectName = "Monitor de Segurança";
    }

    public override void Interact() {
        if (nightOnly && !TimeManager.Instance.isNight) {
            UIManager.Instance.ShowTooltip("O sistema está offline durante o dia.");
            return;
        }

        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.HideTooltip();

        CameraSystem.Instance.OpenCameraView();
    }
}