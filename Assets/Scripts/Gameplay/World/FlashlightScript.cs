using UnityEngine;

public class FlashlightScript : InteractableObject
{
    protected override void Awake() {
        base.Awake();
        objectName = "Lanterna";
        tooltipMessage = "E para apanhar Lanterna";
    }

    // ao apanhar a lanterna marcamos a flag no PlayerController e chamamos Show() no FlashlightHUDController.
    // o HUD so fica visivel a partir daqui - antes disto o jogador nem sabe que a lanterna existe na UI
    public override void Interact() {
        PlayerController.Instance.hasFlashlight = true;
        FlashlightHUDController.Instance.Show();
        Destroy(gameObject);
    }
}
