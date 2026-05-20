using UnityEngine;

public class IntelPickup : InteractableObject
{
    public IntelItem item;
    private bool usado = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = item != null ? item.titulo : "Intel";
        tooltipMessage = item != null ? $"E para ler {item.titulo}" : "E para ler Intel";
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