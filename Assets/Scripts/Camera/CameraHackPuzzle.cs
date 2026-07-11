// #my_code - Lógica central do puzzle de câmaras: recriação do cancelamento de ruído por oposição de fase (sin/cos)
﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraHackPuzzle : MonoBehaviour {
    public static CameraHackPuzzle Instance;
    public static int HackLevel = 0; // incrementa a cada câmara desbloqueada (0 a 7)

    [SerializeField] private GameObject rootPanel;

    // onde estão as barras dos sinais
    [SerializeField] private RectTransform jamContainer;
    [SerializeField] private RectTransform playerContainer;

    [SerializeField] private Slider resonanceSlider; // o quão perto está o jogador de concluir o puzzle
    [SerializeField] private TextMeshProUGUI resonanceLabel;
    [SerializeField] private TextMeshProUGUI timerLabel;
    [SerializeField] private TextMeshProUGUI statusLabel;

    // barras que representam os sinais
    [SerializeField] private Image[] jamBars;
    [SerializeField] private Image[] playerBars;
    [SerializeField] private TextMeshProUGUI[] bandArrows; // setas que indicam qual é a barra atualmente a ser mudada pelo jogador

    // número de barras
    private int numBands = 8;
    // multiplicado por deltaTime * 60 dá ~0.6 por segundo —> rápido o suficiente mas controlado (para subir/descer a barra)
    private float scrollStep = 0.01f;

    private float tolerance; // o quão perto pode estar a barra do jogador comparando à original
    private float holdRequired; // tempo requerido seguido que a barra tem de aguentar como certa
    private float timeLimit; // tempo que o jogador tem para resolver o mini jogo de acordo com o seu nível de intelectual
    // estas variáveis definem o quão as barras vermelhas (não concluídas) oscilam em altura e o quão rapidamente se movem
    private float oscillationAmplitude;
    private float oscillationSpeed;

    // cores das barras
    private Color C_Jam = new Color(0.95f, 0.18f, 0.18f, 0.88f);
    private Color C_Player = new Color(0.18f, 0.85f, 0.95f, 0.88f);
    private Color C_Matched = new Color(0.18f, 1.00f, 0.42f, 0.92f);

    private float[] jamPattern; // padrão do tamanho das barras do sinal original
    private float[] playerVals; // valores que o jogador terá de meter em cada barra para espelhar o inverso das barras do sinal original
    private int selectedBand = 0; // barra atualmente a ser mudada pelo jogador
    private float holdTimer; // quantidade de tempo que todas as barras têm de ficar corretas para passar o mini jogo
    private float countdown;
    [HideInInspector] public bool active;
    private bool finished;
    private int hackingCameraIndex = 0;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        resonanceSlider.minValue = 0f;
        resonanceSlider.maxValue = 1f;
        resonanceSlider.interactable = false;
        rootPanel.SetActive(false);
    }

    void Update() {
        if (!active || finished) return;

        // para sair do menu
        if (Input.GetKeyDown(KeyCode.X)) {
            StartCoroutine(Cancel());
            return;
        }

        countdown -= Time.deltaTime;
        if (countdown <= 0f) {
            StartCoroutine(Fail());
            return;
        }

        HandleKeyboardNavigation();
        HandleKeyboardAdjust();
        RefreshVisuals();
        EvaluateResonance();
    }

    public void Open(int camIndex) {
        hackingCameraIndex = camIndex;
        ApplyDifficulty(HackLevel); // aplicamos o nível do mini jogo de acordo com a quantidade de câmaras já hackeadas

        jamPattern = GeneratePattern(camIndex); // criamos o padrão do tamanho das barras do sinal original
        playerVals = new float[numBands];

        for (int i = 0; i < numBands; i++)
            playerVals[i] = 0.5f; // começamos todas na metade do tamanho da barra

        holdTimer = 0f;
        countdown = timeLimit;
        finished = false;
        active = true;
        selectedBand = 0;

        rootPanel.SetActive(true);

        float jamH = jamContainer.rect.height;
        float playerH = playerContainer.rect.height;

        for (int i = 0; i < numBands; i++) {
            jamBars[i].color = C_Jam;
            playerBars[i].color = C_Player;
            SetBarHeight(jamBars[i], 0f, jamH);
            SetBarHeight(playerBars[i], 0f, playerH);
            bandArrows[i].gameObject.SetActive(i == selectedBand); // só mostramos a seta da barra atualmente a ser mudada
        }

        statusLabel.text = "OBJETIVO: torna o azul o espelho invertido do vermelho";
    }

    private void ApplyDifficulty(int level) {
        // t vai de 0 (câmara 1) a 1 (câmara 8)
        float t = Mathf.Clamp01(level / 7f);

        tolerance = Mathf.Lerp(0.13f, 0.055f, t); // tolerância aperta
        timeLimit = Mathf.Lerp(40f, 28f, t); // menos tempo
        timeLimit += PlayerStats.Instance.GetIntelecto() * 1.5f; // atributo intelecto aumenta o tempo disponível para o minijogo

        holdRequired = Mathf.Lerp(1.0f, 2.8f, t); // mais tempo a manter
        oscillationAmplitude = Mathf.Lerp(0f, 0.07f, t); // JAM começa estático, oscila mais
        oscillationSpeed = Mathf.Lerp(0.4f, 2.2f, t); // oscilação acelera
    }

    // para o padrão ser igual para a mesma câmara caso o jogador a falhe à primeira
    private float[] GeneratePattern(int seed) {
        Random.InitState(seed);
        float[] p = new float[numBands];

        for (int i = 0; i < numBands; i++)
            p[i] = Random.Range(0.16f, 0.84f);

        return p;
    }

    // isto faz com que as barras oscilem ou seja aumentem ou diminuem de altura
    // o band * 0.9 é para cada barra oscilar numa fase ligeiramente diferente do que as outras
    // para que não fiquem a mexerem-se todas em sincronia
    private float GetJamValue(int band) {
        float osc = oscillationAmplitude * Mathf.Sin(Time.time * oscillationSpeed + band * 0.9f);
        return Mathf.Clamp01(jamPattern[band] + osc);
    }

    private float InverseOf(int band) {
        return 1f - GetJamValue(band);
    }

    private void SetBarHeight(Image bar, float normalized, float containerH) {
        RectTransform rt = bar.rectTransform;
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

    // velocidade a que a barra muda de altura de acordo com a quantidade de tempo que a tecla fica premida
    private void HandleKeyboardAdjust() {
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        if (!up && !down)
            return;

        float dir = up ? 1f : -1f;
        playerVals[selectedBand] = Mathf.Clamp01(playerVals[selectedBand] + dir * scrollStep * Time.deltaTime * 60f);
    }

    // isto vai a cada barra e vê o quão perto a barra do jogador está face à barra original se estiver dentro da tolerância conta como 1, senão menos
    // depois divide pelo numero de barras para ter uma percentagem de 0 a 1 e com essa ressonância atualiza os visuais
    // as barras vermelhas tremem se a ressonância for baixa
    // as barras azuis ficam verdes e a pulsar quando estão certas
    private void RefreshVisuals() {
        float jamH = jamContainer.rect.height;
        float playerH = playerContainer.rect.height;

        // primeira passagem: calcular a ressonância global
        float resonance = 0f;
        for (int i = 0; i < numBands; i++) {
            float diff = Mathf.Abs(playerVals[i] - InverseOf(i)); // abs porque só queremos saber o valor total real positivo da diferença

            if (diff <= tolerance)
                resonance += 1f;
            else
                resonance += Mathf.Clamp01(1f - diff / tolerance);
        }

        resonance /= numBands;

        // segunda passagem: atualizar os visuais de cada barra
        bool glitching = resonance < 0.35f && oscillationAmplitude > 0f;

        for (int i = 0; i < numBands; i++) {
            float jamVal = GetJamValue(i);
            float diff = Mathf.Abs(playerVals[i] - (1f - jamVal)); // para que seja sempre positivo porque só queremos saber o valor real total
            bool matched = diff <= tolerance;

            // barra vermelha com glitch quando ressonância é baixa
            float glitch = 0f;
            if (glitching)
                glitch = Random.Range(-0.018f, 0.018f); // tremor pequeno —> o mesmo valor que o chromaMaxOffset

            SetBarHeight(jamBars[i], Mathf.Clamp01(jamVal + glitch), jamH);
            jamBars[i].color = C_Jam;

            // barra azul do jogador
            SetBarHeight(playerBars[i], playerVals[i], playerH);

            if (matched) {
                // pulsa entre verde e branco quando está na posição certa
                float pulse = (Mathf.Sin(Time.time * 6f + i) + 1f) * 0.5f; // 6 rad/s dá ~1 pulso por segundo, o + i faz cada barra pulsar desfasada
                playerBars[i].color = Color.Lerp(C_Matched, Color.white, pulse * 0.4f); // vai só até 40% de branco para não perder a cor verde

            } else {
                playerBars[i].color = C_Player;
            }
        }

        // atualizar slider e label com a percentagem de ressonância
        resonanceSlider.value = resonance;

        Color resColor;
        if (resonance < 0.4f)
            resColor = new Color(0.95f, 0.18f, 0.18f); // vermelho
        else if (resonance < 0.75f)
            resColor = new Color(1.00f, 0.85f, 0.10f); // amarelo
        else
            resColor = new Color(0.18f, 1.00f, 0.42f); // verde

        resonanceLabel.color = resColor;
        resonanceLabel.text = "RESSONÂNCIA  " + Mathf.RoundToInt(resonance * 100f) + "%";
        timerLabel.text = Mathf.CeilToInt(countdown) + "s";
    }

    // isto vê se todas as barras estão dentro da tolerância ao mesmo tempo
    // se sim começa a contar o contador de tempo porque o jogador tem que ter tudo igual durante esse tempo para passar no puzzle
    private void EvaluateResonance() {
        bool allMatched = true;

        for (int i = 0; i < numBands; i++) {
            if (Mathf.Abs(playerVals[i] - InverseOf(i)) > tolerance) {
                allMatched = false;
                break;
            }
        }

        if (allMatched) {
            holdTimer += Time.deltaTime;
            statusLabel.text = "A MANTER RESSONÂNCIA  " + holdTimer.ToString("F1") + " / " + holdRequired.ToString("F1");

            if (holdTimer >= holdRequired && !finished)
                StartCoroutine(Succeed());

        } else {
            // se o jogador já não tiver as barras todas corretas o contador volta a regressar ao 0 1.5x mais depressa do que sobe para penalizar sair da posição
            holdTimer = Mathf.Max(0f, holdTimer - Time.deltaTime * 1.5f);
        }
    }

    // muda a UI com base no resultado do jogador
    private IEnumerator Succeed() {
        finished = true;
        active = false;
        HackLevel++;

        // som de hack bem-sucedido
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.startHackCamera);

        statusLabel.text = "SINAL ANULADO  ·  ACESSO CONCEDIDO";
        resonanceLabel.text = "RESSONÂNCIA  100%";
        resonanceLabel.color = new Color(0.18f, 1f, 0.42f);
        resonanceSlider.value = 1f;

        yield return new WaitForSeconds(1.4f);

        rootPanel.SetActive(false);
        CameraSystem.Instance.UnlockCamera(hackingCameraIndex);
        PlayerController.Instance.canMoveRotate = true;
    }

    private IEnumerator Fail() {
        finished = true;
        active = false;
        statusLabel.text = "SINAL PERDIDO  ·  ACESSO NEGADO";

        // som de hack falhado
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.buzzerWrong);

        SuspicionManager.Instance.IncreaseSuspicion(2f, GetInstanceID(), SuspicionManager.SuspicionSource.Hacking);
        yield return new WaitForSeconds(1.2f);

        rootPanel.SetActive(false);
        CameraViewUI.Instance.UpdateLockState();
        PlayerController.Instance.canMoveRotate = true;
    }

    private IEnumerator Cancel() {
        finished = true;
        active = false;
        rootPanel.SetActive(false);

        CameraViewUI.Instance.UpdateLockState();
        PlayerController.Instance.canMoveRotate = true;

        yield break;
    }
}