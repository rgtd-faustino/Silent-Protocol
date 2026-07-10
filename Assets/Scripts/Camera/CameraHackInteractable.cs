using UnityEngine;

public class CameraHackInteractable : InteractableObject {
    [SerializeField] private int cameraIndex = 0; // indice desta câmara no array allCameras do CameraSystem

    protected override void Awake() {
        base.Awake();
        objectName = "Câmara de Vigilância";
        tooltipMessage = "E para hackear Câmara de Vigilância";
    }

    public override void Interact() {
        // se já estiver desbloqueada, não há puzzle
        if (CameraSystem.Instance.IsUnlocked(cameraIndex)) {
            UIManager.Instance.ShowTooltip("Câmara já desbloqueada.");
            return;
        }

        // para o movimento do jogador para não se mexer enquanto interaje com ela
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.HideTooltip();

        // abre o puzzle e ao completar desbloqueia a cãmara no CameraSystem
        CameraHackPuzzle.Instance.Open(cameraIndex);
    }

}