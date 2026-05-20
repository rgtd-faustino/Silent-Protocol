using UnityEngine;

public class FlashlightScript : InteractableObject
{
    protected override void Awake() {
        base.Awake();
        objectName = "Lanterna";
        tooltipMessage = "E para apanhar Lanterna";
    }

    public override void Interact() {
        PlayerController.Instance.hasFlashlight = true;
        FlashlightHUDController.Instance.Show();
        Destroy(gameObject);
    }
}
