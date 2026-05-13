using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraHackPuzzle : MonoBehaviour {
    public static CameraHackPuzzle Instance;
    public static int HackLevel = 0; // incrementa a cada câmara desbloqueada (0 a 7)

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Signal Containers")]
    [SerializeField] private RectTransform jamContainer;
    [SerializeField] private RectTransform playerContainer;

    [Header("HUD")]
    [SerializeField] private Slider resonanceSlider;
    [SerializeField] private TextMeshProUGUI resonanceLabel;
    [SerializeField] private TextMeshProUGUI timerLabel;
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Jam Bars")]
    [SerializeField] private Image[] jamBars;
    [Header("Player Bars")]
    [SerializeField] private Image[] playerBars;
    [SerializeField] private TextMeshProUGUI[] bandArrows;

    private int numBands = 8;
    private float scrollStep = 0.01f;

    // Configurados dinamicamente por ApplyDifficulty
    private float tolerance;
    private float holdRequired;
    private float timeLimit;
    private float oscillationAmplitude;
    private float oscillationSpeed;

    private static readonly Color C_Jam = new Color(0.95f, 0.18f, 0.18f, 0.88f);
    private static readonly Color C_Player = new Color(0.18f, 0.85f, 0.95f, 0.88f);
    private static readonly Color C_Matched = new Color(0.18f, 1.00f, 0.42f, 0.92f);

    private float[] jamPattern;
    private float[] playerVals;
    private int selectedBand = 0;
    private float holdTimer;
    private float countdown;
    private float lastResonance; // usada para glitch sem loop duplo
    private bool active;
    private bool finished;
    public bool IsOpen => active;
    private Action onSuccess;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        resonanceSlider.minValue = 0f;
        resonanceSlider.maxValue = 1f;
        resonanceSlider.interactable = false;
        rootPanel.SetActive(false);
    }

    void Update() {
        if (!active || finished) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X)) {
            StartCoroutine(Cancel());
            return;
        }

        countdown -= Time.deltaTime;
        if (countdown <= 0f) { StartCoroutine(Fail()); return; }

        HandleKeyboardNavigation();
        HandleKeyboardAdjust();
        RefreshVisuals();
        EvaluateResonance();
    }


    public void Open(int camIndex, Action successCallback) {
        onSuccess = successCallback;
        ApplyDifficulty(HackLevel);

        jamPattern = GeneratePattern(camIndex);
        playerVals = new float[numBands];
        for (int i = 0; i < numBands; i++) playerVals[i] = 0.5f;

        holdTimer = 0f;
        lastResonance = 0f;
        countdown = timeLimit;
        finished = false;
        active = true;

        rootPanel.SetActive(true);

        float jamH = jamContainer.rect.height;
        float playerH = playerContainer.rect.height;
        for (int i = 0; i < numBands; i++) {
            jamBars[i].color = C_Jam;
            playerBars[i].color = C_Player;
            SetHeight(jamBars[i], 0f, jamH);
            SetHeight(playerBars[i], 0f, playerH);
        }

        for (int i = 0; i < numBands; i++) {
            bandArrows[i].gameObject.SetActive(i == selectedBand);
        }

        selectedBand = 0;
        resonanceLabel.color = Color.white;
        statusLabel.text = "OBJETIVO: torna o azul o espelho invertido do vermelho";
    }

    public void ForceClose() {
        active = false;
        rootPanel.SetActive(false);
    }


    private void ApplyDifficulty(int level) {
        // t vai de 0 (câmara 1) a 1 (câmara 8)
        float t = Mathf.Clamp01(level / 7f);

        tolerance = Mathf.Lerp(0.13f, 0.055f, t); // tolerância aperta
        timeLimit = Mathf.Lerp(60f, 28f, t); // menos tempo
        holdRequired = Mathf.Lerp(1.0f, 2.8f, t); // mais tempo a manter
        oscillationAmplitude = Mathf.Lerp(0f, 0.07f, t); // JAM começa estático, oscila mais
        oscillationSpeed = Mathf.Lerp(0.4f, 2.2f, t); // oscilação acelera
    }


    private float[] GeneratePattern(int seed) {
        var rng = new System.Random(seed * 7919 + 1337);
        var p = new float[numBands];
        for (int i = 0; i < numBands; i++)
            p[i] = (float)(rng.NextDouble() * 0.68 + 0.16);
        return p;
    }

    // Valor atual do JAM com oscilação — cada banda tem fase diferente
    private float GetJamValue(int band) {
        float osc = oscillationAmplitude * Mathf.Sin(Time.time * oscillationSpeed + band * 0.9f);
        return Mathf.Clamp01(jamPattern[band] + osc);
    }

    private float InverseOf(int band) => 1f - GetJamValue(band);

    private void SetHeight(Image bar, float normalized, float containerH) {
        var rt = bar.rectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, normalized * containerH);
    }


    private void HandleKeyboardNavigation() {
        int prev = selectedBand;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            selectedBand = (selectedBand - 1 + numBands) % numBands;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            selectedBand = (selectedBand + 1) % numBands;

        if (selectedBand != prev) {
            bandArrows[prev].gameObject.SetActive(false);
            bandArrows[selectedBand].gameObject.SetActive(true);
        }
    }

    private void HandleKeyboardAdjust() {
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        if (!up && !down) return;
        float dir = up ? 1f : -1f;
        playerVals[selectedBand] =
            Mathf.Clamp01(playerVals[selectedBand] + dir * scrollStep * Time.deltaTime * 60f);
    }


    private void RefreshVisuals() {
        float jamH = jamContainer.rect.height;
        float playerH = playerContainer.rect.height;
        float t = Time.time;

        // Primeira passagem: calcular ressonância global (usada para efeito de glitch)
        float resonance = 0f;
        for (int i = 0; i < numBands; i++) {
            float diff = Mathf.Abs(playerVals[i] - InverseOf(i));
            resonance += (diff <= tolerance) ? 1f : Mathf.Clamp01(1f - diff / tolerance);
        }
        resonance /= numBands;
        lastResonance = resonance;

        // Segunda passagem: atualizar visuais
        bool glitching = resonance < 0.35f && oscillationAmplitude > 0f;

        for (int i = 0; i < numBands; i++) {
            float jamVal = GetJamValue(i);
            float target = 1f - jamVal;
            float diff = Mathf.Abs(playerVals[i] - target);
            bool matched = diff <= tolerance;

            // JAM bar com glitch quando ressonância é baixa
            float glitch = glitching ? UnityEngine.Random.Range(-0.018f, 0.018f) : 0f;
            SetHeight(jamBars[i], Mathf.Clamp01(jamVal + glitch), jamH);
            jamBars[i].color = C_Jam;

            // Player bar
            SetHeight(playerBars[i], playerVals[i], playerH);

            if (matched) {
                float pulse = (Mathf.Sin(t * 6f + i) + 1f) * 0.5f;
                playerBars[i].color = Color.Lerp(C_Matched, Color.white, pulse * 0.4f);
            } else {
                playerBars[i].color = C_Player;
            }
        }

        // Slider e label de ressonância
        resonanceSlider.value = resonance;

        Color resColor = resonance < 0.4f ? new Color(0.95f, 0.18f, 0.18f) :
                         resonance < 0.75f ? new Color(1.00f, 0.85f, 0.10f) :
                                             new Color(0.18f, 1.00f, 0.42f);
        resonanceLabel.color = resColor;
        resonanceLabel.text = $"RESSONÂNCIA  {Mathf.RoundToInt(resonance * 100f)}%";

        // Timer vermelho quando < 10s
        timerLabel.color = countdown < 10f ? new Color(1f, 0.2f, 0.2f) : Color.white;
        timerLabel.text = $"{Mathf.CeilToInt(countdown):00}s";
    }


    private void EvaluateResonance() {
        bool allMatched = true;
        for (int i = 0; i < numBands; i++)
            if (Mathf.Abs(playerVals[i] - InverseOf(i)) > tolerance) { allMatched = false; break; }

        if (allMatched) {
            holdTimer += Time.deltaTime;
            statusLabel.text = $"A MANTER RESSONÂNCIA  {holdTimer:F1} / {holdRequired:F1}";
            if (holdTimer >= holdRequired && !finished)
                StartCoroutine(Succeed());
        } else {
            holdTimer = Mathf.Max(0f, holdTimer - Time.deltaTime * 1.5f);
        }
    }


    private IEnumerator Succeed() {
        finished = active = false;
        HackLevel++;
        statusLabel.text = "SINAL ANULADO  ·  ACESSO CONCEDIDO";
        resonanceLabel.text = "RESSONÂNCIA  100%";
        resonanceLabel.color = new Color(0.18f, 1f, 0.42f);
        resonanceSlider.value = 1f;
        yield return new WaitForSeconds(1.4f);
        rootPanel.SetActive(false);
        onSuccess.Invoke();
    }

    private IEnumerator Fail() {
        finished = active = false;
        statusLabel.text = "SINAL PERDIDO  ·  ACESSO NEGADO";
        SuspicionManager.Instance.IncreaseSuspicion(3f, GetInstanceID(), SuspicionManager.SuspicionSource.Hacking);
        yield return new WaitForSeconds(1.2f);
        rootPanel.SetActive(false);
        CameraViewUI.Instance.ShowLockedState();
        PlayerController.Instance.canMoveRotate = true;
    }

    private IEnumerator Cancel() {
        finished = active = false;
        rootPanel.SetActive(false);
        CameraViewUI.Instance.ShowLockedState();
        PlayerController.Instance.canMoveRotate = true;
        yield break;
    }
}