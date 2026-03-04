using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour {
    public static TimeManager Instance;

    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private float daySpeed = 1f; // minutos do jogo por segundo real (dia)
    [SerializeField] private float nightSpeed = 2f; // noite é 2x mais rápida

    [HideInInspector] public bool isNight = false;
    private float currentMinutes = 0f; // minutos do jogo
    private float dayStartMinute = 480f; // 8:00 (08:00 = 8*60)
    private float nightStartMinute = 1320f; // 22:00
    private float dayEndMinute = 1440f; // 24:00 (fim do dia)

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        currentMinutes = dayStartMinute;
        UpdateDisplay();
    }

    void Update() {
        // determina a velocidade atual (dia ou noite)
        float speedMultiplier = isNight ? nightSpeed : daySpeed;

        // 10 minutos reais = 1 hora no jogo (60 minutos)
        // portanto 1 segundo real = 0.1 minutos do jogo
        currentMinutes += speedMultiplier * 0.1f * Time.deltaTime;

        // se passou para noite
        if (currentMinutes >= nightStartMinute && !isNight) {
            isNight = true;
        }

        // se passou o fim do dia
        if (currentMinutes >= dayEndMinute) {
            currentMinutes = dayStartMinute; // volta ao início do dia
            isNight = false;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay() {
        timeDisplay.text = GetTimeDisplay();
    }


    public float GetCurrentTime() {
        return currentMinutes;
    }

    public string GetTimeDisplay() {
        int hours = (int)(currentMinutes / 60f);
        int minutes = (int)(currentMinutes % 60f);
        // para ficar formatado como as horas do relógio, tostring é para meter dois digitos
        return hours.ToString("00") + ":" + minutes.ToString("00");
    }
}