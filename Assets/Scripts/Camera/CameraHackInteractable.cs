using UnityEngine;

public class CameraHackInteractable : InteractableObject {
    [SerializeField] private int cameraIndex = 0; // indice desta cmara no array allCameras do CameraSystem

    protected override void Awake() {
        base.Awake();
        objectName = "Câmara de Vigilância";
        tooltipMessage = "E para hackear Câmara de Vigilância";
    }

    public override void Interact() {
        // se j estiver desbloqueada, no h puzzle
        if (CameraSystem.Instance.IsUnlocked(cameraIndex)) {
            UIManager.Instance.ShowTooltip("Cmara j desbloqueada.");
            return;
        }

        // para o movimento do jogador
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.HideTooltip();

        // abre o puzzle e ao completar desbloqueia a camara no CameraSystem
        CameraHackPuzzle.Instance.Open(cameraIndex);
    }

}