using System.Collections.Generic;
using TMPro;
using UnityEngine;

// #my_code - Ciclo dia/noite e gestão do estado de fadiga acumulada do jogador
public class TimeManager : MonoBehaviour {

    public static TimeManager Instance;

    // o tempo aparece tanto no HUD normal como dentro do ecrã do computador
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private TextMeshProUGUI timeDisplayInComputer;

    // daySpeed e nightSpeed controlam quantos "minutos de jogo" passam por segundo real, a noite corre mais depressa
    [Header("Velocidade do tempo")]
    private float daySpeed = 1f;
    private float nightSpeed = 2f;
    private float debugSpeedMultiplier = 10f; // 100, testar o jogo mais depressa

    // outros scripts (ex: PlayerController para a lanterna, BedScript para só deixar dormir à noite) consultam esta variável
    [HideInInspector] public bool isNight = false;
    [HideInInspector] public float lastDeltaMinutes;

    private const float DayStartMinute = 480f;   // 08:00 — dia começa
    private const float WorkStartMinute = 540f;   // 09:00 — começam as tarefas
    private const float LunchMinute = 750f;   // 12:30 — pausa de almoço
    private const float AfternoonMinute = 840f;   // 14:00 — tarde de trabalho
    private const float NightStartMinute = 1320f; // 22:00 — começa a noite
    private const float DayEndMinute = 1440f;  // 24:00 — fim do dia

    private float currentMinutes = 0f;
    // regista quantas horas de sono em falta o jogador acumulou. Cresce com o tempo (estar acordado desgasta) e baixa quando o jogador dorme
    private float accumulatedSleep = 0f;
    // conta quantos cafés foram bebidos para calcular o "vício", ou seja, quanto mais cafés, menor o efeito de cada um
    private int coffeesTaken = 0;

    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    // flags para que cada evento do dia (trabalho, almoço, etc.) só dispare uma vez
    private bool firedWorkStart = false;
    private bool firedLunch = false;
    private bool firedAfternoon = false;
    private bool firedNight = false;

    private const float MeetingMinute = 1050f; // 17:30
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
        // O jogo começa às 08:00
        SetCurrentMinutes(DayStartMinute);
        UpdateDisplay();
    }

    void Update() {
        // a velocidade depende se é dia ou noite
        float speed = isNight ? nightSpeed : daySpeed;
        float deltaMinutes = speed * 0.1f * Time.deltaTime * debugSpeedMultiplier;
        lastDeltaMinutes = deltaMinutes;
        currentMinutes += deltaMinutes;

        // convertemos para horas para que o limiar de 7 horas no sistema de fadiga seja mais fácil de configurar
        accumulatedSleep += deltaMinutes / 60f;

        // para que o tempo dê reset ao fim de 24 horas
        if (currentMinutes >= DayEndMinute)
        {
            if (!firedDayEnd)
            {
                firedDayEnd = true;
                GameEvent.DayEnded();
            }
            currentMinutes %= DayEndMinute;
            ResetDayFlags(); // firedDayStart fica false — vai disparar às 08:00
                             
        }

        isNight = currentMinutes >= NightStartMinute || currentMinutes < DayStartMinute;

        // verificar se algum marco do dia foi atingido e disparar o evento correspondente
        FireDayEvents();

        // atualizar cada efeito de status ativo
        // percorremos ao contrário para poder remover elementos da lista durante a iteração (para não dar erros ao mexer em arrays sob alteração durante loops)
        for (int i = activeEffects.Count - 1; i >= 0; i--) {
            if (activeEffects[i].UpdateEffect(deltaMinutes))
                activeEffects.RemoveAt(i);
        }

        UpdateDisplay();
    }


    // em vez do TimeManager chamar diretamente o TaskManager ou o NPCManager quando chega a hora do almoço, dispara um evento global via GameEvent
    // qualquer script que precise de reagir a esses momentos subscreve esse evento, o TimeManager não sabe (nem precisa de saber) quem são esses scripts
    private void FireDayEvents() {
        if (!firedWorkStart && currentMinutes >= WorkStartMinute) {
            firedWorkStart = true;
            GameEvent.WorkHoursStarted(); // avisa o TaskManager para criar as tarefas do dia
        }
        if (!firedLunch && currentMinutes >= LunchMinute) {
            firedLunch = true;
            GameEvent.LunchStarted(); // usado para rotinas de NPC, futuras cutscenes, etc.
        }
        if (!firedAfternoon && currentMinutes >= AfternoonMinute) {
            firedAfternoon = true;
            GameEvent.AfternoonStarted(); // avisa o TaskManager para criar as tarefas da tarde
        }
        if (!firedNight && currentMinutes >= NightStartMinute) {
            firedNight = true;
            GameEvent.NightStarted(); // avisa o PlayerController (lanterna), NPCs, etc.
        }
        if (!firedMeeting && currentMinutes >= MeetingMinute) {
            firedMeeting = true;
            NPCManager.Instance.TriggerMeeting(); // avisa o GameManager para chamar todos os NPCs para a reunião
        }
        if (!firedDayStart && currentMinutes >= DayStartMinute)
        {
            firedDayStart = true;
            Debug.Log($"FireDayEvents: DayStart disparado! Dia atual: {DayManager.Instance.CurrentDay}");
            DayManager.Instance.OnDayEnded();//para dar a daymanager disparar o envento de dizer que o dia mudou
        }
    }
        
    // reseta as flags no início de um novo dia (chamado após Sleep)
    private void ResetDayFlags() {
        
        firedWorkStart = firedLunch = firedAfternoon = firedNight = firedMeeting = firedDayEnd = firedDayStart = false;
    }


    // calcula quantas horas o jogador ainda pode dormir antes das 08:00, usado pelo UIManager para validar a hora de acordar introduzida
    // pelo jogador (não pode acordar depois do início do dia de trabalho)
    public float GetMaxSleepHours() {
        if (currentMinutes < DayStartMinute)
            return (DayStartMinute - currentMinutes) / 60f;
        else
            return (DayEndMinute - currentMinutes + DayStartMinute) / 60f;
    }

    // formata os minutos internos como "HH:MM" para mostrar no ecrã
    public string GetTimeDisplay() {
        int h = (int)(currentMinutes / 60f);
        int m = (int)(currentMinutes % 60f);
        return $"{h:00}:{m:00}"; // :00 garante sempre dois dígitos (ex: "08:05" em vez de "8:5")
    }

    public float GetCurrentTimeInHours() => currentMinutes / 60f;

    public void SetCurrentMinutes(float minutes) {
        currentMinutes = minutes;
    }


    // aplica os efeitos de uma noite de sono: avança o tempo para a hora de acordar e recalcula a fadiga acumulada
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


    // o café é um StatusEffect temporário que melhora o "sono efetivo" sem alterar o sono real
    // o efeito diminui com o vício (coffeesTaken)
    // a lógica do efeito é passada como uma função lambda para que o TimeManager não precise de saber os detalhes
    // e apenas aplique o que o efeito disser
    public void Coffee() {
        int baseStage = GetSleepStage();

        // café não faz nada se o jogador não está com fadiga.
        if (baseStage == 0)
            return;

        coffeesTaken++;

        // quanto mais cafés bebidos (vício), menor a duração do efeito
        // mínimo de 10 minutos para que nunca seja completamente inútil
        float duration = Mathf.Max(10f, 120f - GetCoffeeStage() * 30f);

        // a função lambda recebe o estágio real (baseStage), o estágio atual com outros efeitos (currentStage) e o tempo decorrido desde que o efeito começou (timerMinutes)
        // retorna o estágio modificado pelo café naquele momento
        StatusEffect coffeeEffect = new StatusEffect(duration, (realStage, currentStage, timerMinutes) => {
            if (realStage == 1) return 0; // fadiga leve: café elimina completamente

            if (realStage == 2) {
                // fadiga moderada: café resolve no início, piora no fim
                if (timerMinutes < duration * 0.25f) 
                    return 1;
                if (timerMinutes < duration * 0.75f) 
                    return 0;

                return 1;
            }

            if (realStage == 3) {
                // fadiga severa: café atenua no início, piora depois
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

    // nível de vício em café: 0 a 3.
    public int GetCoffeeStage() {
        return Mathf.Min(coffeesTaken / 3, 3);
    }

    // o estágio de sono real, calculado apenas com base na fadiga acumulada
    // os limiares (19h, 42h, 50h) representam horas de sono em dívida,
    // não horas acordado, mas "desfasamento" entre sono ideal e sono real
    // 0 = sem fadiga, 1 = leve, 2 = moderada, 3 = severa.
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

    // o que o jogo usa para consequências (animações lentas, erros, etc.)
    // não é o sono real, mas o sono "percebido" — depois de aplicar todos os efeitos ativos (café, etc.).
    // separá-lo do GetSleepStage garante que o café não "engana" a fadiga real, só mascara os efeitos temporariamente
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



    // para converter o tempo real usado no tempo de espera por waypoint
    public float ToRealSeconds(float gameMinutes) {
        float speed = isNight ? nightSpeed : daySpeed;
        return gameMinutes / (speed * 0.1f * debugSpeedMultiplier);
    }

    // getters e setters para o SaveManager
    public float GetCurrentMinutes() { return currentMinutes; }
    public float GetAccumulatedSleep() { return accumulatedSleep; }
    public int GetCoffeesTaken() { return coffeesTaken; }

    public void SetAccumulatedSleep(float value) { accumulatedSleep = value; }
    public void SetCoffeesTaken(int value) { coffeesTaken = value; }
}