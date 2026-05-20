using UnityEngine;

public class IntelPickup : InteractableObject
{
    public IntelItem item;
    private bool usado = false;

    void Start()
    {
        objectName = item != null ? item.titulo : "Intel";
    }

    public override void Interact()
    {
        if (usado) return;
        usado = true;

        UIManager.Instance.HideTooltip();

        IntelReadUI.Instance.AbrirLeitura(
            item,
            callbackGuardar: () => Destroy(gameObject),
            callbackIgnorar: () =>
            {
                usado = false;
            }
        );
    }
}   