using UnityEngine;

public class CardCredentialPickup : InteractableObject
{
    public string cardID;
    public string cardName;
    public string notificationText = "Cartão de acesso obtido: ";

    // o dia a partir do qual o cartão spawna no mapa, o DayManager gere o dia atual e ativa os cartões quando chegamos lá
    public int diaParaAparecer = 1;

    private bool isPickedUp = false;

    // registamos logo no Awake o listener do OnDayChanged
    protected override void Awake()
    {
        base.Awake();
        objectName = cardName;
        tooltipMessage = $"E para apanhar {cardName}";

        GameEvent.OnDayChanged += HandleDayChanged;
    }

    private void OnDestroy()
    {
        GameEvent.OnDayChanged -= HandleDayChanged;
    }

    private void Start()
    {
        // para mostrarmos apenas quando for suposto, fazemos isto logo no início para desativar/ativar todos os que forem necessários
        gameObject.SetActive(DayManager.Instance.CurrentDay >= diaParaAparecer);
    }

    private void HandleDayChanged(int day)
    {
        if (day >= diaParaAparecer)
            gameObject.SetActive(true);
    }

    // este método injeta o ID do cartão diretamente no PlayerController, os CardReaders depois vão validar se o ID está na lista do jogador
    public override void Interact()
    {
        if (isPickedUp) 
            return;

        isPickedUp = true;

        PlayerController.Instance.AddCardCredential(cardID);
        Debug.Log($"[CardCredentialPickup] Cartão recolhido: {cardName} (ID: {cardID})");

        UIManager.Instance.HideTooltip();

        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }

    // forçamos os cartões a brilhar sempre que não forem apanhados para o jogador não andar às cegas a procurar no cenário
    protected override bool CheckShouldGlowByDefault()
    {
        return !isPickedUp;
    }
}