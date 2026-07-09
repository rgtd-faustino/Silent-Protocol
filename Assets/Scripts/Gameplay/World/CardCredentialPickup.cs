using UnityEngine;

public class CardCredentialPickup : InteractableObject
{
    public string cardID;
    public string cardName;

    public string notificationText = "Cartão de acesso obtido: ";

    [Header("Visibilidade por dia")]
    public int diaParaAparecer = 1; // igual ao IntelPickup, define no Inspector por objeto

    private bool isPickedUp = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = cardName;
        tooltipMessage = $"E para apanhar {cardName}";

        // subscreve aqui, não desativa o objeto neste ponto — Awake corre sempre até ao fim
        GameEvent.OnDayChanged += HandleDayChanged;
    }

    private void OnDestroy()
    {
        GameEvent.OnDayChanged -= HandleDayChanged;
    }

    private void Start()
    {
        // aqui sim, decide a visibilidade inicial — Start já corre depois de todos os Awake da cena,
        // por isso DayManager.Instance já está garantidamente atribuído
        gameObject.SetActive(DayManager.Instance.CurrentDay >= diaParaAparecer);
    }

    private void HandleDayChanged(int day)
    {
        if (day >= diaParaAparecer)
            gameObject.SetActive(true);
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