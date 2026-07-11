using UnityEngine;

public class FlashlightController : MonoBehaviour
{

    public static FlashlightController Instance;

    [Header("Referências")]
    [SerializeField] private Light flashlight;

    [Header("Bateria")]
    [SerializeField] private float maxBattery = 90f;
    private float currentBattery;

    public bool isOn = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentBattery = maxBattery;
        flashlight.enabled = false;

        // subescrevemos o evento de início da noite para garantirmos que a bateria reseta a cada novo turno
        GameEvent.OnNightStarted += OnNightStarted;
    }

    void OnDestroy()
    {
        GameEvent.OnNightStarted -= OnNightStarted;
    }

    // só usamos a lanterna quando é de noite
    void Update()
    {
        if (!TimeManager.Instance.isNight)
        {
            if (isOn) TurnOff();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
            Toggle();

        // vamos gastando a bateria consoante os minutos de jogo fornecidos pelo TimeManager para garantir que o consumo acompanha o tempo do jogo
        if (isOn)
        {
            float deltaMinutes = TimeManager.Instance.lastDeltaMinutes;

            currentBattery = Mathf.Max(0f, currentBattery - deltaMinutes);
            if (currentBattery <= 0f)
            {
                TurnOff();
                Debug.Log("[Lanterna] Bateria esgotada.");
            }
        }
    }

    private void Toggle()
    {
        if (isOn)
            TurnOff();
        else
            TurnOn();

        if (TutorialManager.Instance.IsCurrentStepGate("tut_flashlight"))
        {
            TutorialManager.Instance.CompleteCurrentStep();
        }

        if (PlayerController.Instance.hasFlashlight)
            SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.flashlightToggleOnOff);
    }

    // o NPCScript reage ao estado de ligada para aumentar a probabilidade do jogador ser detetado
    public void TurnOn()
    {
        // bloqueamos a luz se for dia ou se o jogador não a tiver
        if (currentBattery <= 0f || PlayerController.Instance.hasFlashlight == false || !TimeManager.Instance.isNight)
            return;

        isOn = true;
        flashlight.enabled = true;
    }

    public void TurnOff()
    {
        isOn = false;
        flashlight.enabled = false;
    }

    private void OnNightStarted()
    {
        TurnOff();
        currentBattery = maxBattery;
        Debug.Log("[Lanterna] Bateria recarregada.");
    }

    // exposto para o FlashlightHUDController ir apanhar a informação e conseguir atualizar as barras visuais
    public float GetBatteryRatio()
    {
        return currentBattery / maxBattery;
    }

    public void SetBatteryRatio(float ratio)
    {
        currentBattery = ratio * maxBattery;
    }

    // repõe a bateria ao máximo e desliga a lanterna, para que um "Novo Jogo" comece do zero
    public void ResetForNewGame()
    {
        currentBattery = maxBattery;
        isOn = false;
        flashlight.enabled = false;

        Debug.Log("[Lanterna] Estado reiniciado para um novo jogo.");
    }
}