using UnityEngine;

public class CameraHackInteractable : InteractableObject {
    [SerializeField] private int cameraIndex = 0; // indice desta câmara no array allCameras do CameraSystem

    void Start() {
        objectName = "Câmara de Vigilância";
    }

    public override void Interact() {
        // se já estiver desbloqueada, não há puzzle
        if (CameraSystem.Instance.IsUnlocked(cameraIndex)) {
            UIManager.Instance.ShowTooltip("Câmara já desbloqueada.");
            return;
        }

        // para o movimento do jogador
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.HideTooltip();

        // abre o puzzle e ao completar desbloqueia a camara no CameraSystem
        CameraHackPuzzle.Instance.Open(cameraIndex);
    }

}