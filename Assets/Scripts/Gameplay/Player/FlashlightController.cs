using UnityEngine;

// ligar a lanterna aumenta o alcance de visão dos NPCs (ver NPCScript.IsPlayerInFOV)
// e o nível de suspeita gerado por segundo quando o jogador é visto (ver NPCScript.FOVCheckRoutine)
// a bateria drena em tempo real enquanto está ligada; recarrega completamente ao início de cada nova noite
public class FlashlightController : MonoBehaviour {

    public static FlashlightController Instance;

    [Header("Referências")]
    [SerializeField] private Light flashlight;

    [Header("Bateria")]
    // minutos de jogo, a noite tem 600, mas o jogador não precisa da luz a noite toda
    [SerializeField] private float maxBattery = 90f; // 9 segundos reais com debug = 50, senão 7.5 minutos reais
    private float currentBattery;

    public bool isOn = false;

    void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    void Start() {
        currentBattery = maxBattery;
        flashlight.enabled = false;

        // recarregar bateria no início de cada novo dia (após dormir)
        GameEvent.OnNightStarted += OnNightStarted;
    }

    void OnDestroy() {
        GameEvent.OnNightStarted -= OnNightStarted;
    }

    void Update() {
        // a lanterna só funciona à noite
        /*if (!TimeManager.Instance.isNight) {
            if (IsOn) TurnOff();
            return;
        }*/

        if (Input.GetKeyDown(KeyCode.F))
            Toggle();

        // gastar bateria enquanto está ligada
        if (isOn) {
            float deltaMinutes = TimeManager.Instance.lastDeltaMinutes;

            currentBattery = Mathf.Max(0f, currentBattery - deltaMinutes);
            if (currentBattery <= 0f) {
                TurnOff();
                Debug.Log("[Lanterna] Bateria esgotada.");
            }
        }
    }

    private void Toggle() {
        if (isOn) 
            TurnOff(); 
        else 
            TurnOn();
    }

    public void TurnOn() {
        if (currentBattery <= 0f || PlayerController.Instance.hasFlashlight == false) 
            return;
        isOn = true;

        flashlight.enabled = true;
    }

    public void TurnOff() {
        isOn = false;
        flashlight.enabled = false;
    }

    // recarrega a bateria a cada nova noite
    private void OnNightStarted() {
        TurnOff();
        currentBattery = maxBattery;
        Debug.Log("[Lanterna] Bateria recarregada.");
    }

    // 0 = sem bateria, 1 = cheia —> usado pelo HUD para mostrar barra de bateria
    public float GetBatteryRatio() {
        return currentBattery / maxBattery;
    }

    // setter para o SaveManager restaurar a bateria
    public void SetBatteryRatio(float ratio) {
        currentBattery = ratio * maxBattery;
    }
}
