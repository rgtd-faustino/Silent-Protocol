using UnityEngine;

// #my_code - Recolha de intel: sistema central de progressão do jogo
public class IntelPickup : InteractableObject
{
    public IntelItem item;

    [Header("Visibilidade por dia")]
    public int diaParaAparecer = 1; // defines no Inspector por objeto

    private bool usado = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = item != null ? item.titulo : "Intel";
        tooltipMessage = item != null ? $"E para ler {item.titulo}" : "E para ler Intel";

        // come�a invis�vel
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvent.OnDayChanged += HandleDayChanged;
    }

    private void OnDisable()
    {
        GameEvent.OnDayChanged -= HandleDayChanged;
    }

    private void Start()
    {
        // verifica o dia actual quando a cena carrega
        // para o caso de o objeto j� dever estar vis�vel
        HandleDayChanged(DayManager.Instance.CurrentDay);
    }

    private void HandleDayChanged(int day)
    {
        if (day >= diaParaAparecer)
            gameObject.SetActive(true);
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