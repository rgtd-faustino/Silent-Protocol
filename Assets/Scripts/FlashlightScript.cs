using UnityEngine;

public class FlashlightScript : InteractableObject
{
    public override void Interact() {
        PlayerController.Instance.hasFlashlight = true;
        FlashlightHUDController.Instance.Show();
        Destroy(gameObject);
    }
}
