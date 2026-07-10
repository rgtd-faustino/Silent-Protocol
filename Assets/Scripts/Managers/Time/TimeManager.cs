using System.Collections.Generic;
using TMPro;
using UnityEngine;

// #my_code - Ciclo dia/noite e gestão do estado de fadiga acumulada do jogador
public class TimeManager : MonoBehaviour {

    public static TimeManager Instance;

    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private TextMeshProUGUI timeDisplayInComputer;

    [Header("Velocidade do tempo")]
    private float daySpeed = 1f;
    // a noite passa o dobro da velocidade para não secar o jogador com sessões noturnas longas, já que há menos coisas para fazer
    private float nightSpeed = 2f;
    private float debugSpeedMultiplier = 10f;

    [HideInInspector] public bool isNight = false;
    [HideInInspector] public float lastDeltaMinutes;

    private const float DayStartMinute = 480f;
    private const float WorkStartMinute = 540f;
    private const float LunchMinute = 750f;
    private const float AfternoonMinute = 840f;
    private const float NightStartMinute = 1320f;
    private const float DayEndMinute = 1440f;

    private float currentMinutes = 0f;

    // decidimos acumular as horas de sono em dívida em vez de usar uma barra tradicional de energia. 
    // isto sobe sempre que o jogador está acordado e desce quando ele dorme. o GetSleepStage() lê isto para decidir a fadiga
    private float accumulatedSleep = 0f;

    // conta os cafés consumidos para simular o vício. quanto mais o jogador abusar, menos tempo dura o efeito do próximo café
    private int coffeesTaken = 0;

    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    private bool firedWorkStart = false;
    private bool firedLunch = false;
    private bool firedAfternoon = false;
    private bool firedNight = false;

    private const float MeetingMinute = 1050f;
    private bool firedMeeting = false;
    private bool firedDayEnd = false;
    private bool firedDayStart = true;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    void Start() {
        SetCurrentMinutes(DayStartMinute);
        UpdateDisplay();
    }

    void Update() {
        float speed = isNight ? nightSpeed : daySpeed;
        float deltaMinutes = speed * 0.1f * Time.deltaTime * debugSpeedMultiplier;
        lastDeltaMinutes = deltaMinutes;
        currentMinutes += deltaMinutes;

        accumulatedSleep += deltaMinutes / 60f;

        if (currentMinutes >= DayEndMinute)
        {
            if (!firedDayEnd)
            {
                firedDayEnd = true;
                GameEvent.DayEnded();
            }
            currentMinutes %= DayEndMinute;
            ResetDayFlags();
        }

        isNight = currentMinutes >= NightStartMinute || currentMinutes < DayStartMinute;

        FireDayEvents();

        // iteramos a lista de trás para a frente para podermos remover os efeitos terminados sem partir os índices do ciclo
        for (int i = activeEffects.Count - 1; i >= 0; i--) {
            if (activeEffects[i].UpdateEffect(deltaMinutes))
                activeEffects.RemoveAt(i);
        }

        UpdateDisplay();
    }

    // preferimos usar o padrão Observer com o GameEvent para disparar os momentos do dia.
    // assim não precisamos de acoplar o TimeManager ao TaskManager ou ao NPCManager, quem precisar apenas subscreve os eventos
    private void FireDayEvents() {
        if (!firedWorkStart && currentMinutes >= WorkStartMinute) {
            firedWorkStart = true;
            GameEvent.WorkHoursStarted();
        }
        if (!firedLunch && currentMinutes >= LunchMinute) {
            firedLunch = true;
            GameEvent.LunchStarted();
        }
        if (!firedAfternoon && currentMinutes >= AfternoonMinute) {
            firedAfternoon = true;
            GameEvent.AfternoonStarted();
        }
        if (!firedNight && currentMinutes >= NightStartMinute) {
            firedNight = true;
            GameEvent.NightStarted();
        }
        if (!firedMeeting && currentMinutes >= MeetingMinute) {
            firedMeeting = true;
            NPCManager.Instance.TriggerMeeting();
        }
        if (!firedDayStart && currentMinutes >= DayStartMinute)
        {
            firedDayStart = true;
            Debug.Log($"FireDayEvents: DayStart disparado! Dia atual: {DayManager.Instance.CurrentDay}");
            DayManager.Instance.OnDayEnded();
        }
    }

    private void ResetDayFlags() {
        firedWorkStart = firedLunch = firedAfternoon = firedNight = firedMeeting = firedDayEnd = firedDayStart = false;
    }

    // devolve o tempo máximo que o jogador ainda pode dormir antes de começar o trabalho.
    // o UIManager consome isto para impedir que o jogador defina alarmes para depois do início do expediente
    public float GetMaxSleepHours() {
        if (currentMinutes < DayStartMinute)
            return (DayStartMinute - currentMinutes) / 60f;
        else
            return (DayEndMinute - currentMinutes + DayStartMinute) / 60f;
    }

    public string GetTimeDisplay() {
        int h = (int)(currentMinutes / 60f);
        int m = (int)(currentMinutes % 60f);
        return $"{h:00}:{m:00}";
    }

    public float GetCurrentTimeInHours() => currentMinutes / 60f;

    public void SetCurrentMinutes(float minutes) {
        currentMinutes = minutes;
    }

    // avança o tempo automaticamente para a hora de acordar e abate a dívida de sono.
    // se dormir mais de 7 horas zera a dívida, caso contrário recupera apenas uma percentagem para penalizar noitadas
    public void Sleep(float wakeUpHours)
    {
        float wakeUpMinutes = wakeUpHours * 60f;
        float sleepHours;

        if (wakeUpMinutes > currentMinutes)
            sleepHours = (wakeUpMinutes - currentMinutes) / 60f;
        else
            sleepHours = ((1440f - currentMinutes) + wakeUpMinutes) / 60f;

        if (sleepHours >= 7f)
            accumulatedSleep = 0f;
        else
        {
            float recoveryRatio = sleepHours / 7f;
            accumulatedSleep = Mathf.Max(0f, accumulatedSleep * (1f - recoveryRatio));
        }

        SetCurrentMinutes(wakeUpMinutes);
        ResetDayFlags();
        firedDayStart = true;
        Debug.Log($"Dormiu {sleepHours:F1}h. Sono acumulado: {accumulatedSleep:F2}h");

        GameEvent.DayEnded();
        DayManager.Instance.OnDayEnded();
    }

    // aplica o efeito do café usando uma função anónima (lambda) passada ao StatusEffect.
    // isto permite manter a lógica do crash de cafeína contida aqui sem complicar o construtor do StatusEffect noutros ficheiros.
    // a duração base cai com o vício mas pusemos um mínimo empírico de 10 minutos para não ser inútil no endgame
    public void Coffee() {
        int baseStage = GetSleepStage();

        if (baseStage == 0)
            return;

        coffeesTaken++;

        float duration = Mathf.Max(10f, 120f - GetCoffeeStage() * 30f);

        StatusEffect coffeeEffect = new StatusEffect(duration, (realStage, currentStage, timerMinutes) => {
            if (realStage == 1) return 0;

            if (realStage == 2) {
                if (timerMinutes < duration * 0.25f)
                    return 1;
                if (timerMinutes < duration * 0.75f)
                    return 0;

                return 1;
            }

            if (realStage == 3) {
                if (timerMinutes < duration * 0.5f)
                    return 1;
                if (timerMinutes < duration * 0.75f)
                    return 0;

                return 2;
            }

            return realStage;
        });

        activeEffects.Add(coffeeEffect);
        Debug.Log($"Café bebido às {GetTimeDisplay()}. Duração: {duration}min. Vício: {GetCoffeeStage()}");
    }

    public int GetCoffeeStage() {
        return Mathf.Min(coffeesTaken / 3, 3);
    }

    // calcula o estágio real de fadiga baseado apenas na dívida de sono, ignorando se o jogador bebeu café.
    // o atributo de resistência de PlayerStats altera os limiares para tornar a progressão mais flexível consoante a build
    public int GetSleepStage() {
        float resistanceBonus = 0f;
        if (PlayerStats.Instance != null) {
            resistanceBonus = (PlayerStats.Instance.GetResistencia() - 1) * 2f;
        }

        if (accumulatedSleep < 19f + resistanceBonus) return 0;
        if (accumulatedSleep < 42f + resistanceBonus) return 1;
        if (accumulatedSleep < 50f + resistanceBonus) return 2;
        return 3;
    }

    // este é o método que os outros sistemas (animações, UI) chamam para saber a fadiga, porque aqui já conta com os disfarces tipo o café
    public int GetEffectiveSleepStage() {
        int baseStage = GetSleepStage();
        int stage = baseStage;

        foreach (var effect in activeEffects) {
            if (effect.timer < effect.duration)
                stage = effect.modifySleepStage(baseStage, stage, effect.timer);
        }

        return Mathf.Clamp(stage, 0, 3);
    }

    private void UpdateDisplay() {
        string time = GetTimeDisplay();
        timeDisplay.text = time;
        timeDisplayInComputer.text = time;
    }

    public float ToRealSeconds(float gameMinutes) {
        float speed = isNight ? nightSpeed : daySpeed;
        return gameMinutes / (speed * 0.1f * debugSpeedMultiplier);
    }

    public float GetCurrentMinutes() { return currentMinutes; }
    public float GetAccumulatedSleep() { return accumulatedSleep; }
    public int GetCoffeesTaken() { return coffeesTaken; }

    public void SetAccumulatedSleep(float value) { accumulatedSleep = value; }
    public void SetCoffeesTaken(int value) { coffeesTaken = value; }
}