using UnityEngine;
using TMPro;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour {
    public static TimeManager Instance;

    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private float daySpeed = 1f; // minutos do jogo por segundo real (dia)
    [SerializeField] private float nightSpeed = 2f; // noite é 2x mais rápida
    private float debugSpeedMultiplier = 200f;// so para ver se esta a funcionar depois tirar!
    [HideInInspector] public bool isNight = false;

    private float currentMinutes = 0f; // minutos do jogo
    private float dayStartMinute = 480f; // 8:00 (08:00 = 8*60)
    private float nightStartMinute = 1320f; // 22:00
    private float dayEndMinute = 1440f; // 24:00 (fim do dia) , n sei se faz sentido

    private float accumulatedSleep; // variavel para ver o tempo que ele está acordado sem dormir

    private int coffeTaken = 0;
    private float speedMultiplier;
    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    private float sleepPrintTimer = 0f; // contador para print do sono
    private float sleepPrintInterval = 5f; // print a cada 5 segundos




    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        SetCurrentMinutes(dayStartMinute);
        UpdateDisplay();
    }

    void Update()
    {
        float deltaMinutes = (isNight ? nightSpeed : daySpeed) * 0.1f * Time.deltaTime * debugSpeedMultiplier;
        currentMinutes += deltaMinutes;
        accumulatedSleep += deltaMinutes / 60f;
        if (currentMinutes >= dayEndMinute)
        {
            currentMinutes = 0f;
        }

        
        currentMinutes %= 1440f;

        // Atualiza efeitos
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].UpdateEffect(deltaMinutes))
                activeEffects.RemoveAt(i);
        }

        // print sono a cada 5s
        sleepPrintTimer += Time.deltaTime;
        if (sleepPrintTimer >= sleepPrintInterval)
        {
            sleepPrintTimer = 0f;
            Debug.Log($"=== SLEEP DEBUG ===");
            Debug.Log($"Hora atual: {GetTimeDisplay()}");
            Debug.Log($"accumulatedSleep: {accumulatedSleep:F2}h");
            Debug.Log($"isNight: {isNight}");
            Debug.Log($"SleepStage real: {GetSleepStage()}");
            Debug.Log($"SleepStage efetivo: {GetEffectiveSleepStage()}");
            Debug.Log($"Efeitos ativos: {activeEffects.Count}");
        }

        UpdateDisplay();
    }


    public float GetMaxSleepHours() {
        if (currentMinutes < dayStartMinute)
            return (dayStartMinute - currentMinutes) / 60f;
        else
            return (dayEndMinute - currentMinutes + dayStartMinute) / 60f;
    }

    private void UpdateDisplay() {
        timeDisplay.text = GetTimeDisplay();
    }




    public string GetTimeDisplay() {
        int hours = (int)(currentMinutes / 60f);
        int minutes = (int)(currentMinutes % 60f);
        // para ficar formatado como as horas do relógio, tostring é para meter dois digitos
        return hours.ToString("00") + ":" + minutes.ToString("00");
    }




    // Fiz esta funcao para ajudar no playerController dormir, assim qq que seja a hora se ele for dormir acorda as 8h.
    public void SetCurrentMinutes(float time) {
        currentMinutes = time;
    }




    //funcao para dormir
    public void TrySleep(float sleepHours = 0) {
        float time = currentMinutes;
        float wakeUp = dayStartMinute; // 8:00
        if (isNight)
        {
            if (sleepHours == 0)
            {
                // calcular quantas horas dormiu
                if (time < wakeUp)
                    sleepHours = (wakeUp - time) / 60f;
                else
                    sleepHours = ((dayEndMinute - time) + wakeUp) / 60f;
            }



            float effectiveSleep = Mathf.Min(accumulatedSleep, 24f);

            //se dormires mais que 7 horas as horas acumuladas voltam a 0 
            if (sleepHours >= 7f)
            {
                accumulatedSleep = 0f;

            }
            else
            {
                accumulatedSleep = effectiveSleep - sleepHours;

                if (accumulatedSleep < 0)
                    accumulatedSleep = 0;
            }



            SetCurrentMinutes(wakeUp);

            Debug.Log("Dormiu " + sleepHours + " horas");
            Debug.Log("Sono acumulado: " + accumulatedSleep);
        }
        else
        {
            Debug.Log("Secalhar n posso dormir!");
        }
        
    }

    // a funcao cafe é uma funcao para conseguirmos fazer algum trabalho mesmo estando com sono. O objetivo da funao é durante 
    //, inicialemnte 2 horas, ficar sem cansaso, porem n tira o cansanso so o camufla, quando essas horas passam o jogador volta
    //ao estagio normal.
    //Quero fazer isto gradualmente. Se ele tiver no estagio 1 de sono ele passa diretamente para o 0 e quando as horas acabarem
    //passa denovo para o 1 porem se tiver no estagio 2 nos primeiros 30 min vai para o estagio 1, a hora depois vai para o 0 e
    //os ultimos 30 min volta a tar para o estagio 1 terminando as 2 horas no estagio 2 denovo.

    // No ultimo estagio n passamos para o estagio 2 vamos para o 1, mas demora mais tempo para chegar ao estagio 0. Ficas 1 hora no estagio
    //1, meio no estagio 0, e o resto no estagio 2 ate acabar o efeito voltando ao 3.

    //Vamos meter tbm um vicio, a cada 3 cafés bebidos avanca o seu estagio de vicio. Cada estagio tira 30 min de camuflagem.
    public void Coffe()
    {
        int baseStage = GetSleepStage();
        if (baseStage == 0) return;

        coffeTaken++;

        float coffeeStartMinute = currentMinutes; // minuto do jogo em que bebeu o café
        float durationMinutes = 120f - GetCoffeStage() * 30f; // duração em minutos de jogo
        if (durationMinutes < 10f) durationMinutes = 10f;

        StatusEffect coffeeEffect = new StatusEffect(durationMinutes, (realStage, currentStage, timerMinutes) =>
        {
            // timerMinutes já é minutos de jogo — sem necessidade de calcular elapsed
            if (realStage == 1) return 0;

            if (realStage == 2)
            {
                if (timerMinutes < durationMinutes * 0.25f) return 1;
                if (timerMinutes < durationMinutes * 0.75f) return 0;
                return 1;
            }

            if (realStage == 3)
            {
                if (timerMinutes < durationMinutes * 0.5f) return 1;
                if (timerMinutes < durationMinutes * 0.75f) return 0;
                return 2;
            }

            return realStage;
        });

        activeEffects.Add(coffeeEffect);

        Debug.Log($"Café bebido às {GetTimeDisplay()}. Duration: {durationMinutes}min. Vício Stage: {GetCoffeStage()}");
    }



    public void Sleep()
    {
        Debug.Log($"=== ANTES DE DORMIR ===");
        Debug.Log($"accumulatedSleep: {accumulatedSleep:F2}h");
        Debug.Log($"SleepStage: {GetSleepStage()}");
        Debug.Log($"Hora: {GetTimeDisplay()}");
        if (isNight)
        {
            float time = currentMinutes;
            float wakeUp = dayStartMinute; // 8:00
            float sleepHours;

            // calcular quantas horas dormiu
            if (time < wakeUp)
                sleepHours = (wakeUp - time) / 60f;
            else
                sleepHours = ((1440f - time) + wakeUp) / 60f;


            if (sleepHours >= 7f)
            {
                accumulatedSleep = 0f;
            }
            else
            {
                // Cada hora de sono "recupera" mais do que 1 hora de vigília
                // Ex: 6h de sono limpa 6 * (accumulatedSleep / 7f) horas acumuladas
                float recoveryRatio = sleepHours / 7f;
                accumulatedSleep *= (1f - recoveryRatio);

                if (accumulatedSleep < 0)
                    accumulatedSleep = 0;
            }

            // totalMinutesAwake = 0f;

            SetCurrentMinutes(wakeUp);
            
            Debug.Log("Dormiu " + sleepHours + " horas");
            Debug.Log("Sono acumulado: " + accumulatedSleep);

            Debug.Log($"=== DEPOIS DE DORMIR ===");
            Debug.Log($"sleepHours dormidas: {sleepHours:F2}h");
            Debug.Log($"accumulatedSleep: {accumulatedSleep:F2}h");
            Debug.Log($"SleepStage: {GetSleepStage()}");

        }
      
        /* else
         {
             Debug.Log("Secalhar n posso dormir");
         }*/

    }

    //efeito do vicio do cafe
    public int GetCoffeStage()
    {
        if (coffeTaken <3) return 0;
        if (coffeTaken >=3 && coffeTaken <6) return 1;
        if (coffeTaken >=6 && coffeTaken<9) return 2;
        return 3;
    }
    // fazer as coisas da camara 
    public int GetSleepStage() {
        if (accumulatedSleep < 19) return 0;
        if (accumulatedSleep < 42) return 1;
        if (accumulatedSleep < 50) return 2;

        return 3;
    }
    public int GetEffectiveSleepStage()
    {
        int baseStage = GetSleepStage();
        int stage = baseStage;

        foreach (var effect in activeEffects)
        {
            if (effect.timer < effect.duration)
            {
                stage = effect.modifySleepStage(baseStage, stage, effect.timer);
            }
        }

        return Mathf.Clamp(stage, 0, 3);
    }




}