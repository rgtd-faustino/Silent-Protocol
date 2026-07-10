using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MenuState { None, MainMenu, CharacterCreation, Playing, Paused }

public class GameMenuManager : MonoBehaviour {
    public static GameMenuManager Instance;

    [Header("Painéis - cada um precisa de um CanvasGroup")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup charCreationPanel;
    [SerializeField] private CanvasGroup pausePanel;

    [Header("Cards centrais - CanvasGroup em LoginCard / FormCard / PauseCard")]
    [SerializeField] private CanvasGroup mainMenuCard;
    [SerializeField] private CanvasGroup charCreationCard;
    [SerializeField] private CanvasGroup pauseCard;

    [Header("Delay entre BG e card (segundos)")]
    [SerializeField] private float cardRevealDelay = 0.15f;

    [Header("Menu Principal")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Criação de Personagem")]
    [SerializeField] private StatRow[] statRows;
    [SerializeField] private TextMeshProUGUI remainingPointsText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private int totalExtraPoints = 28;
    [SerializeField] private Color pipOn = Color.white;
    [SerializeField] private Color pipOff = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private float pipAnimDelay = 0.04f;

    [Header("Menu de Pausa")]
    [SerializeField] private TextMeshProUGUI pauseDayTimeText;
    [SerializeField] private TextMeshProUGUI pauseSuspicionText;
    [SerializeField] private TextMeshProUGUI pauseBatteryText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveExitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button abandonButton;

    [Header("Efeito Typewriter")]
    [SerializeField] private string titleFullText = "AUTHENTICATION\nREQUIRED";
    private float charDelay = 0.045f;
    private float cursorBlinkRate = 0.52f;
    [SerializeField] private string cursorChar = "_";

    [Header("Scanlines Menu")]
    [SerializeField] private RawImage menuScanlines;
    private float menuScanlineSpeed = 0.2f;

    [Header("Transições")]
    private float fadeDuration = 0.25f;
    private float cardStartScale = 0.92f;
    private float cardEntranceDuration = 0.35f;

    public MenuState CurrentState = MenuState.None;

    private bool isTransitioning = false;
    private Coroutine typewriterCoroutine;

    // criámos este array paralelo para guardar o estado real enquanto a UI faz as suas animações
    private int[] statValues;
    private int remaining;

    [System.Serializable]
    public struct StatRow {
        public string id;
        public TextMeshProUGUI valueText;
        public Button minusButton;
        public Button plusButton;
        public Image[] pips;
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        mainMenuPanel.gameObject.SetActive(false);
        charCreationPanel.gameObject.SetActive(false);
        pausePanel.gameObject.SetActive(false);

        WireButtons();

        continueButton.interactable = SaveManager.Instance.HasSave();

        GoTo(MenuState.MainMenu);
    }

    void Update() {
        float y = menuScanlines.uvRect.y + menuScanlineSpeed * Time.unscaledDeltaTime;
        if (y > 1f) y -= 1f;
        menuScanlines.uvRect = new Rect(0f, y, 1f, 1f);

        float pulse = Mathf.Sin(Time.unscaledTime * 1.2f) * 0.5f + 0.5f;
        Color sc = menuScanlines.color;
        sc.a = Mathf.Lerp(0.12f, 0.28f, pulse);
        menuScanlines.color = sc;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (CameraSystem.Instance != null && CameraSystem.Instance.isActive) return;
            if (CurrentState == MenuState.Playing)
                GoTo(MenuState.Paused);
            else if (CurrentState == MenuState.Paused)
                GoTo(MenuState.Playing);
        }
    }

    public void ResumeGame() {
        GoTo(MenuState.Playing);
    }

    public void PauseGame() {
        GoTo(MenuState.Paused);
    }

    public void GoToMainMenu() {
        GoTo(MenuState.MainMenu);
    }

    // centralizamos as transições aqui para evitar bugs gráficos em que as corrotinas de UI tentam atropelar-se com inputs rápidos
    private void GoTo(MenuState next) {
        if (isTransitioning)
            return;

        StartCoroutine(Transition(next));
    }

    private IEnumerator Transition(MenuState next) {
        isTransitioning = true;

        CanvasGroup outPanel = GetPanelFor(CurrentState);
        if (outPanel != null) {
            yield return StartCoroutine(Fade(outPanel, outPanel.alpha, 0f));
            outPanel.gameObject.SetActive(false);
        }

        CurrentState = next;
        OnEnter(next);

        CanvasGroup inPanel = GetPanelFor(next);
        CanvasGroup inCard = GetCardFor(next);

        if (inPanel != null) {
            inPanel.gameObject.SetActive(true);
            inPanel.alpha = 0f;
            inPanel.interactable = false;
            inPanel.blocksRaycasts = false;

            if (inCard != null) {
                inCard.alpha = 0f;
                inCard.interactable = false;
                inCard.blocksRaycasts = false;

                inCard.transform.localScale = Vector3.one * cardStartScale;
            }

            yield return StartCoroutine(Fade(inPanel, 0f, 1f));
            yield return new WaitForSecondsRealtime(cardRevealDelay);

            if (inCard != null)
                yield return StartCoroutine(FadeAndScale(inCard, 0f, 1f, cardStartScale, 1f));
        }

        isTransitioning = false;
    }

    private void OnEnter(MenuState state) {
        bool isPlaying = (state == MenuState.Playing);

        Cursor.lockState = isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isPlaying;

        if (PlayerController.Instance != null)
            PlayerController.Instance.canMoveRotate = isPlaying;

        switch (state) {
            case MenuState.MainMenu:
                continueButton.interactable = SaveManager.Instance.HasSave();

                SoundManager.Instance.PlayMenuMusic();

                PlayTypewriter();
                break;

            case MenuState.CharacterCreation:
                SoundManager.Instance.PlayMenuMusic();
                InitCharacterCreation();
                break;

            case MenuState.Playing:
                SoundManager.Instance.PlayGameplayMusic();
                break;

            case MenuState.Paused:
                SoundManager.Instance.PlayMenuMusic();
                RefreshPauseUI();
                break;
        }
    }

    // o typewriter assíncrono melhora imenso o feel do ecrã de login inicial, escondendo que o texto era estático
    private void PlayTypewriter() {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterSequence());
    }

    private IEnumerator TypewriterSequence() {
        titleText.text = "";
        foreach (char c in titleFullText) {
            titleText.text += c;
            yield return new WaitForSecondsRealtime(charDelay);
        }

        typewriterCoroutine = StartCoroutine(BlinkCursor());
    }

    private IEnumerator BlinkCursor() {
        bool cursorVisible = true;
        while (true) {
            titleText.text = cursorVisible ? titleFullText + cursorChar : titleFullText;
            cursorVisible = !cursorVisible;
            yield return new WaitForSecondsRealtime(cursorBlinkRate);
        }
    }

    private void InitCharacterCreation() {
        statValues = new int[statRows.Length];
        for (int i = 0; i < statValues.Length; i++)
            statValues[i] = 1;

        remaining = totalExtraPoints;

        RefreshStats(false);
    }

    public void AddFor() {
        AdjustStat(0, +1);
    }
    public void SubFor() {
        AdjustStat(0, -1);
    }

    public void AddPer() {
        AdjustStat(1, +1);
    }
    public void SubPer() {
        AdjustStat(1, -1);
    }

    public void AddRes() {
        AdjustStat(2, +1);
    }
    public void SubRes() {
        AdjustStat(2, -1);
    }

    public void AddCar() {
        AdjustStat(3, +1);
    }
    public void SubCar() {
        AdjustStat(3, -1);
    }

    public void AddInt() {
        AdjustStat(4, +1);
    }
    public void SubInt() {
        AdjustStat(4, -1);
    }

    public void AddAgi() {
        AdjustStat(5, +1);
    }
    public void SubAgi() {
        AdjustStat(5, -1);
    }

    public void AddSor() {
        AdjustStat(6, +1);
    }
    public void SubSor() {
        AdjustStat(6, -1);
    }

    private void AdjustStat(int idx, int delta) {
        int newValue = statValues[idx] + delta;

        if (newValue < 1 || newValue > 10)
            return;
        if (delta > 0 && remaining <= 0)
            return;

        statValues[idx] = newValue;
        remaining -= delta;

        RefreshStats(true, changedIdx: idx);
    }

    private void RefreshStats(bool animated, int changedIdx = -1) {
        remainingPointsText.text = remaining + " PONTOS DISPONÍVEIS";

        for (int i = 0; i < statRows.Length; i++) {
            if (statRows[i].valueText != null)
                statRows[i].valueText.text = statValues[i].ToString();

            statRows[i].minusButton.interactable = (statValues[i] > 1);
            statRows[i].plusButton.interactable = (statValues[i] < 10 && remaining > 0);

            if (animated && i == changedIdx)
                StartCoroutine(AnimatePips(i));
            else
                for (int p = 0; p < statRows[i].pips.Length; p++)
                    statRows[i].pips[p].color = (p < statValues[i]) ? pipOn : pipOff;
        }
    }

    private IEnumerator AnimatePips(int idx) {
        for (int p = 0; p < statRows[idx].pips.Length; p++) {
            statRows[idx].pips[p].color = (p < statValues[idx]) ? pipOn : pipOff;
            yield return new WaitForSecondsRealtime(pipAnimDelay);
        }
    }

    private void ConfirmStats() {
        PlayerStats.Instance.SetStats(statValues);
        GoTo(MenuState.Playing);
    }

    private void ResetStats() {
        InitCharacterCreation();
    }

    // o painel de pausa lê os dados de diferentes managers e consolida isto tudo.
    // assim não espalhamos a lógica da UI por mil ficheiros
    private void RefreshPauseUI() {
        int currentDay = GameManager.Instance.currentDay;
        float timeHours = TimeManager.Instance.GetCurrentTimeInHours();

        int hours = Mathf.FloorToInt(timeHours);
        int minutes = Mathf.FloorToInt((timeHours - hours) * 60f);

        pauseDayTimeText.text = $"DIA {currentDay}  //  {hours:00}:{minutes:00}";

        string suspicionLabel = "SUSPEITA: NENHUMA";

        SuspicionManager.SuspicionState state = SuspicionManager.Instance.GetCurrentState();

        switch (state) {
            case SuspicionManager.SuspicionState.None:
                suspicionLabel = "SUSPEITA: NENHUMA";
                break;
            case SuspicionManager.SuspicionState.Attention:
                suspicionLabel = "SUSPEITA: ATENÇÃO";
                break;
            case SuspicionManager.SuspicionState.Investigation:
                suspicionLabel = "SUSPEITA: INVESTIGAÇÃO";
                break;
            case SuspicionManager.SuspicionState.Expulsion:
                suspicionLabel = "SUSPEITA: EXPULSÃO";
                break;
        }

        pauseSuspicionText.text = suspicionLabel;

        int batteryPct = Mathf.RoundToInt(FlashlightController.Instance.GetBatteryRatio() * 100f);
        pauseBatteryText.text = $"BATERIA: {batteryPct}%";
    }

    // mapeamos os botões todos via código no Start. é menos propenso a erros de quebra de referências no Unity Editor se mudarmos a estrutura do Canvas
    private void WireButtons() {
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        confirmButton.onClick.AddListener(ConfirmStats);
        resetButton.onClick.AddListener(ResetStats);

        resumeButton.onClick.AddListener(ResumeGame);
        saveExitButton.onClick.AddListener(OnSaveExitClicked);
        abandonButton.onClick.AddListener(OnAbandonClicked);

        AddClickSound(newGameButton);
        AddClickSound(continueButton);
        AddClickSound(quitButton);
        AddClickSound(confirmButton);
        AddClickSound(resetButton);
        AddClickSound(resumeButton);
        AddClickSound(saveExitButton);
        AddClickSound(abandonButton);

        foreach (StatRow row in statRows) {
            AddClickSound(row.minusButton);
            AddClickSound(row.plusButton);
        }
    }

    private void AddClickSound(Button btn) {
        if (btn == null) return;
        btn.onClick.AddListener(() =>
            SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.buttonClick)
        );
    }

    public void OnNewGameClicked() {
        SaveManager.Instance.DeleteSave();
        GoTo(MenuState.CharacterCreation);
    }

    private void OnContinueClicked() {
        SaveData data = SaveManager.Instance.Load();
        SaveManager.Instance.ApplySave(data);
        GoTo(MenuState.Playing);
    }

    public void OnQuitClicked() {
        Application.Quit();
    }

    private void OnSaveExitClicked() {
        SaveManager.Instance.Save();
        continueButton.interactable = true;
        GoTo(MenuState.MainMenu);
    }

    private void OnAbandonClicked() {
        SaveManager.Instance.DeleteSave();
        continueButton.interactable = false;
        GoTo(MenuState.MainMenu);
    }

    private CanvasGroup GetPanelFor(MenuState state) {
        switch (state) {
            case MenuState.MainMenu:
                return mainMenuPanel;

            case MenuState.CharacterCreation:
                return charCreationPanel;

            case MenuState.Paused:
                return pausePanel;

            default:
                return null;
        }
    }

    private CanvasGroup GetCardFor(MenuState state) {
        switch (state) {
            case MenuState.MainMenu:
                return mainMenuCard;

            case MenuState.CharacterCreation:
                return charCreationCard;

            case MenuState.Paused:
                return pauseCard;

            default:
                return null;
        }
    }


    private IEnumerator Fade(CanvasGroup cg, float from, float to) {
        SetPanel(cg, from, to > 0f);

        float elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        SetPanel(cg, to, to > 0f);
    }

    // decidimos usar o SmoothStep na curva de escalonamento para o fade parecer que as janelas têm massa física.
    // é um pequeno detalhe mas faz a UI do terminal parecer muito melhor e integrada
    private IEnumerator FadeAndScale(CanvasGroup cg, float fromAlpha, float toAlpha,
                                     float fromScale, float toScale) {
        SetPanel(cg, fromAlpha, toAlpha > 0f);

        float elapsed = 0f;
        while (elapsed < cardEntranceDuration) {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / cardEntranceDuration));

            cg.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            cg.transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, t);
            yield return null;
        }

        SetPanel(cg, toAlpha, toAlpha > 0f);
        cg.transform.localScale = Vector3.one * toScale;
    }

    private static void SetPanel(CanvasGroup cg, float alpha, bool interactive) {
        cg.alpha = alpha;
        cg.interactable = interactive;
        cg.blocksRaycasts = interactive;
    }


}