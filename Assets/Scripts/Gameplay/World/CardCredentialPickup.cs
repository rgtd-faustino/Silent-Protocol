using UnityEngine;

public class CardCredentialPickup : InteractableObject
{
    public string cardID;
    public string cardName;

    public string notificationText = "Cartão de acesso obtido: ";

    private bool isPickedUp = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = cardName;
        tooltipMessage = $"E para apanhar {cardName}";
    }

    public override void Interact()
    {
        if (isPickedUp) return;

        isPickedUp = true;
        
        // adicionamos o cartão à lista de credenciais do jogador
        PlayerController.Instance.AddCardCredential(cardID);
        Debug.Log($"[CardCredentialPickup] Cartão recolhido: {cardName} (ID: {cardID})");

        UIManager.Instance.HideTooltip();

        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }

    protected override bool CheckShouldGlowByDefault()
    {
        return !isPickedUp;
    }
}
