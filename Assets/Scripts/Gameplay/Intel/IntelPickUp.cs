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

        GameEvent.OnDayChanged += HandleDayChanged; // inscreve-se aqui, corre sempre
    }

    private void OnDestroy()
    {
        GameEvent.OnDayChanged -= HandleDayChanged; // desinscreve só quando o objeto morre de vez
    }

    private void Start()
    {
        // aqui o objeto ainda está ativo (Awake não o desativou), por isso Start corre normalmente
        gameObject.SetActive(DayManager.Instance.CurrentDay >= diaParaAparecer);
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
        
        // som de pegar a intel
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.intelPickup);

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