using UnityEngine;

public class CameraHackInteractable : InteractableObject {
    [Header("Camera Hack")]
    [Tooltip("Índice desta câmara no array allCameras do CameraSystem.")]
    [SerializeField] private int cameraIndex = 0;

    void Start() {
        objectName = "Câmara de Vigilância";
    }

    public override void Interact() {
        // Se já estiver desbloqueada, não há puzzle
        if (CameraSystem.Instance.IsUnlocked(cameraIndex)) {
            UIManager.Instance.ShowTooltip("Câmara já desbloqueada.");
            return;
        }

        // Para o movimento do jogador
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.HideTooltip();

        // Abre o puzzle — ao completar, desbloqueia no CameraSystem
        CameraHackPuzzle.Instance.Open(cameraIndex, () => {
            CameraSystem.Instance.UnlockCamera(cameraIndex);
            PlayerController.Instance.canMoveRotate = true;
        });
    }
}