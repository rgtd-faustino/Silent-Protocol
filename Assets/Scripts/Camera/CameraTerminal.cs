using UnityEngine;

public class CameraTerminal : InteractableObject
{
    [Header("Camera Terminal")]
    [Tooltip("If true, this terminal is only usable at night (e.g. security desk after hours).")]
    [SerializeField] private bool nightOnly = false;

    [Tooltip("Optional: restrict to a specific floor. -1 = any floor.")]
    [SerializeField] private int requiredFloor = -1;

    void Start()
    {
        objectName = "Monitor de Segurança";
    }

    public override void Interact()
    {
        // Night restriction
        if (nightOnly && TimeManager.Instance != null && !TimeManager.Instance.isNight)
        {
            UIManager.Instance.ShowTooltip("O sistema está offline durante o dia.");
            return;
        }

        // Floor restriction
        if (requiredFloor >= 0 && GameManager.Instance != null &&
            GameManager.Instance.currentFloor != requiredFloor)
        {
            UIManager.Instance.ShowTooltip("Sem acesso a este terminal.");
            return;
        }

        if (CameraSystem.Instance == null)
        {
            Debug.LogWarning("[CameraTerminal] CameraSystem não encontrado na cena.");
            return;
        }

        // Lock player movement while watching cameras
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.HideTooltip();

        CameraSystem.Instance.OpenCameraView();

        Debug.Log("[CameraTerminal] Câmaras abertas.");
    }
}
