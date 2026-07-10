using UnityEngine;

public class CardCredentialPickup : InteractableObject
{
    public string cardID;
    public string cardName;

    public string notificationText = "Cartão de acesso obtido: ";

    [Header("Visibilidade por dia")]
    // o dia a partir do qual o cartão spawna no mapa. o DayManager gere o dia atual e ativa os cartões quando chegamos lá
    public int diaParaAparecer = 1;

    private bool isPickedUp = false;

    // a gente regista logo no Awake o listener do OnDayChanged. assim não há problemas de concorrência com o DayManager que possa atualizar o dia entretanto
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

    // o Start corre depois de todos os managers fazerem o Awake, portanto temos garantia absoluta que a instância do DayManager já existe na memória
    private void Start()
    {
        gameObject.SetActive(DayManager.Instance.CurrentDay >= diaParaAparecer);
    }

    private void HandleDayChanged(int day)
    {
        if (day >= diaParaAparecer)
            gameObject.SetActive(true);
    }

    // este método injeta o ID do cartão diretamente no PlayerController. os CardReaders depois vão validar se o ID está na lista do gajo
    public override void Interact()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        PlayerController.Instance.AddCardCredential(cardID);
        Debug.Log($"[CardCredentialPickup] Cartão recolhido: {cardName} (ID: {cardID})");

        UIManager.Instance.HideTooltip();

        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }

    // forçamos os cartões a brilhar sempre que não foram apanhados para o jogador não andar às cegas a procurar no cenário escuro
    protected override bool CheckShouldGlowByDefault()
    {
        return !isPickedUp;
    }
}