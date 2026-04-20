using UnityEngine;

// Gere a lanterna do jogador durante a noite.
// Ligar a lanterna aumenta o alcance de visão dos NPCs (ver NPCScript.IsPlayerInFOV)
// e o nível de suspeita gerado por segundo quando o jogador é visto (ver NPCScript.FOVCheckRoutine).
// A bateria drena em tempo real enquanto está ligada; recarrega completamente ao dormir / início de novo dia.
public class FlashlightController : MonoBehaviour {

    public static FlashlightController Instance;

    [Header("Referências")]
    // luz Spot filho da câmara — arrastar aqui no Inspector
    [SerializeField] private Light flashlight;

    [Header("Bateria")]
    // duração total em segundos reais (120s ≈ 2 minutos reais com debugSpeedMultiplier normal)
    [SerializeField] private float maxBattery = 120f;
    private float currentBattery;

    [Header("Deteção")]
    // bónus de alcance (metros) que os NPCs ganham quando a lanterna está ligada
    // consultado pelo NPCScript em IsPlayerInFOV
    public float lightDetectionBonus = 10f;

    public bool IsOn { get; private set; } = false;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() {
        currentBattery = maxBattery;

        if (flashlight != null)
            flashlight.enabled = false;

        // recarregar bateria no início de cada novo dia (após dormir)
        GameEvent.OnDayChanged += OnDayChanged;
    }

    void OnDestroy() {
        GameEvent.OnDayChanged -= OnDayChanged;
    }

    void Update() {
        // a lanterna só funciona à noite
        if (!TimeManager.Instance.isNight) {
            if (IsOn) TurnOff();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
            Toggle();

        // drenar bateria enquanto está ligada
        if (IsOn) {
            currentBattery = Mathf.Max(0f, currentBattery - Time.deltaTime);
            if (currentBattery <= 0f) {
                TurnOff();
                Debug.Log("[Lanterna] Bateria esgotada.");
            }
        }
    }

    private void Toggle() {
        if (IsOn) TurnOff(); else TurnOn();
    }

    public void TurnOn() {
        if (currentBattery <= 0f) return;
        IsOn = true;
        if (flashlight != null) flashlight.enabled = true;
    }

    public void TurnOff() {
        IsOn = false;
        if (flashlight != null) flashlight.enabled = false;
    }

    // recarrega a bateria a cada novo dia (chamado após o jogador dormir)
    private void OnDayChanged(int day) {
        TurnOff();
        currentBattery = maxBattery;
        Debug.Log("[Lanterna] Bateria recarregada.");
    }

    // 0 = sem bateria, 1 = cheia — usado pelo HUD para mostrar barra de bateria
    public float GetBatteryRatio() => currentBattery / maxBattery;
}