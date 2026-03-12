using UnityEngine;
using TMPro;
using UnityEditor.SceneManagement;

public class TimeManager : MonoBehaviour {
    public static TimeManager Instance;

    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private float daySpeed = 1f; // minutos do jogo por segundo real (dia)
    [SerializeField] private float nightSpeed = 2f; // noite é 2x mais rápida
    private float debugSpeedMultiplier = 1f;// so para ver se esta a funcionar depois tirar!
    [HideInInspector] public bool isNight = false;

    private float currentMinutes = 0f; // minutos do jogo
    private float dayStartMinute = 480f; // 8:00 (08:00 = 8*60)
    private float nightStartMinute = 1320f; // 22:00
    private float dayEndMinute = 1440f; // 24:00 (fim do dia) , n sei se faz sentido

    private float accumulatedSleep; // variavel para ver o tempo que ele está acordado sem dormir
    /*
    O accumulated Sleep funciona da seguinte maneira se tiveres 8 horas de sono (pelo menos) a variavel retira 10f (8h-22h = 10h)
    ao valor do accumulatedSleep.
    O accumulatedSleep sobe 1 a cada hora que passa do dia.

    O objetivo é ter 3 estagios, se as horas acumuladas ultrapassarem as 24 horas ficas sem poder fazer alguma coisa que ainda n sei oq é
    se ultrapassarem as 48 horas vais tendo black outs randoms, aumentando a tua suspeita, pois os outros precebem.
    se ultrapassares as 56 horas tens 2 majors black outs, onde o tempo passa uns 10 a 15 minutos, oq aumenta em muito a tua suspeita, 
    e ao 3 so dormes e acordas passado 8 horas. Se isso acontecer dependendo do teu nivel de suspeita é gameover.
     */



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

    void Update() {
        // determina a velocidade atual (dia ou noite)
        float speedMultiplier = isNight ? nightSpeed : daySpeed;


        // 10 minutos reais = 1 hora no jogo (60 minutos)
        // portanto 1 segundo real = 0.1 minutos do jogo
        float deltaMinutes = speedMultiplier * 0.1f * Time.deltaTime * debugSpeedMultiplier;

        currentMinutes += deltaMinutes;

        //a cada hora que passou aumenta uma hora no sono acumulado 
        accumulatedSleep += deltaMinutes / 60f;


        if (currentMinutes >= dayEndMinute) {
            currentMinutes = 0f;
        }

        // se passou para noite
        if (currentMinutes >= nightStartMinute && !isNight) {
            isNight = true;

        }

        /*
        // se passou o fim do dia
        if (currentMinutes >= dayEndMinute) {
            currentMinutes = dayStartMinute; // volta ao início do dia
            isNight = false;
        }
        O que isto faz é passar o relogio das 24 diretamente para as 8, mas como o jogador pode ficar acordado a noite não faz sentido estar isto assim
        O que podemos fazer é se os currentMinutes forem = as 8 horas para ser demanha. Pois o jogador n deve de poder dormir no horario de trabalho 
        apartir das 8h da manha ent começa supostamente o dia as 8h.
        Ao tirar isto tive que arranjar um maneira do relogio estar sempre a contar e eu fiz o seguinte, se forem 24h passa a 0 outra vez(No inicio do udpate)
         */

        // quando o relogio esta nas 8h é de dia logo n pode dormir
        if (currentMinutes >= dayStartMinute && currentMinutes < nightStartMinute && isNight) {
            isNight = false;

        }

        switch (GetSleepStage()) {
            case 0:
                Debug.Log("Tou Fixe");
                break;

            case 1:
                Debug.Log("Tou com sono");
                break;

            case 2:
                Debug.Log("Tou cheio de sono");
                break;

            case 3:
                Debug.Log("Vou caputar");
                break;
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

        // pronto uhm eu n entendi bem o que querias fazer aqui mas como o jogador é que decide quantas horas ele vai dormir eu passo por parâmetro o número q o jogador quer
        // mas para não ignorar o teu código eu deixo o aqui à mesma e assim se n meteres um valor no parâmetro o teu código é usado
        if (sleepHours == 0) {
            // calcular quantas horas dormiu
            if (time < wakeUp)
                sleepHours = (wakeUp - time) / 60f;
            else
                sleepHours = ((dayEndMinute - time) + wakeUp) / 60f;
        }


        // eu fiz com que o sono acumulado n for maior que 24 horas pk se ficares acordado 48 horas e dormires tipo 6 horas
        // o sono acumulado fica com 43 acomuladas e isso n era nem realista nem otimo para a gameplay. entao para ficar mais realista 
        //fiz assim 
        float effectiveSleep = Mathf.Min(accumulatedSleep, 24f);

        //se dormires mais que 7 horas as horas acumuladas voltam a 0 
        if (sleepHours >= 7f) {
            accumulatedSleep = 0f;

        } else {
            accumulatedSleep = effectiveSleep - sleepHours;

            if (accumulatedSleep < 0)
                accumulatedSleep = 0;
        }



        SetCurrentMinutes(wakeUp);

        Debug.Log("Dormiu " + sleepHours + " horas");
        Debug.Log("Sono acumulado: " + accumulatedSleep);
    }


    // fazer as coisas da camara 
    public int GetSleepStage() {
        if (accumulatedSleep < 19) return 0;
        if (accumulatedSleep < 42) return 1;
        if (accumulatedSleep < 50) return 2;

        return 3;
    }


}