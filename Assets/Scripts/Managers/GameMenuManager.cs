using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum MenuState { None, MainMenu, CharacterCreation, Playing, Paused } // "None" é usado no arranque antes de qualquer painel estar visível

public class GameMenuManager : MonoBehaviour {
    public static GameMenuManager Instance;

    [Header("Painéis - cada um precisa de um CanvasGroup")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup charCreationPanel;
    [SerializeField] private CanvasGroup pausePanel;

    // os cards são os painéis centrais (LoginCard, FormCard, PauseCard)
    // têm CanvasGroup próprio para que o BG apareça primeiro e o card depois
    [Header("Cards centrais - CanvasGroup em LoginCard / FormCard / PauseCard")]
    [SerializeField] private CanvasGroup mainMenuCard;
    [SerializeField] private CanvasGroup charCreationCard;
    [SerializeField] private CanvasGroup pauseCard;

    [Header("Delay entre BG e card (segundos)")]
    [SerializeField] private float cardRevealDelay = 0.15f;

    [Header("Menu Principal")]
    [SerializeField] private TextMeshProUGUI titleText; // onde o efeito typewriter escreve
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton; // desativado se não houver save
    [SerializeField] private Button quitButton;

    [Header("Criação de Personagem")]
    [SerializeField] private StatRow[] statRows; // uma entrada por atributo (FOR, PER, ...)
    [SerializeField] private TextMeshProUGUI remainingPointsText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private int totalExtraPoints = 28; // pontos livres para distribuir (base é 1 em cada stat)
    [SerializeField] private Color pipOn = Color.white;
    [SerializeField] private Color pipOff = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private float pipAnimDelay = 0.04f; // segundos entre cada pip a acender (animação)

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
    private float charDelay = 0.045f; // segundos entre cada letra
    private float cursorBlinkRate = 0.52f; // segundos entre piscar o cursor
    [SerializeField] private string cursorChar = "_";

    [Header("Scanlines Menu")]
    [SerializeField] private RawImage menuScanlines;
    private float menuScanlineSpeed = 0.2f;

    [Header("Transições")]
    private float fadeDuration = 0.25f; // duração do fade entre painéis
    // o card começa a 92% do tamanho e cresce para 100% enquanto o faz fade in
    private float cardStartScale = 0.92f;
    // duração da animação scale+fade do card, ligeiramente mais longa que o fadeDuration
    // para que o BG já esteja totalmente visível quando o card começa a crescer
    private float cardEntranceDuration = 0.35f;

    public MenuState CurrentState = MenuState.None;

    private bool isTransitioning = false;
    private Coroutine typewriterCoroutine;

    private int[] statValues;
    private int remaining;

    [System.Serializable]
    public struct StatRow {
        public string id; // identificador textual (ex: "FOR"), não é usado para nada, mas convém ter um id
        public TextMeshProUGUI valueText; // mostra o número atual (1-10)
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
        // escondemos todos os painéis caso não estejam já escondidos no editor
        mainMenuPanel.gameObject.SetActive(false);
        charCreationPanel.gameObject.SetActive(false);
        pausePanel.gameObject.SetActive(false);

        // metemos os botões ligados aos métodos pelos códigos porque são muitos botões e assim sabemos que está tudo certo
        WireButtons();

        // começar no menu principal
        GoTo(MenuState.MainMenu);
    }

    void Update() {
        float y = menuScanlines.uvRect.y + menuScanlineSpeed * Time.unscaledDeltaTime;
        if (y > 1f) y -= 1f;
        menuScanlines.uvRect = new Rect(0f, y, 1f, 1f);

        // pulso de opacidade — seno lento para dar o efeito "vivo" igual ao terminal
        float pulse = Mathf.Sin(Time.unscaledTime * 1.2f) * 0.5f + 0.5f; // 0 a 1
        Color sc = menuScanlines.color;
        sc.a = Mathf.Lerp(0.12f, 0.28f, pulse);
        menuScanlines.color = sc;

        // escape pausa/retoma o jogo —> não funciona no menu principal nem na criação de personagem
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (CameraSystem.Instance != null && CameraSystem.Instance.IsActive) return;
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

    private void GoTo(MenuState next) {
        // impede que múltiplos cliques rápidos lancem várias transições em simultâneo
        if (isTransitioning)
            return;

        StartCoroutine(Transition(next));
    }

    private IEnumerator Transition(MenuState next) {
        isTransitioning = true;

        // fade out do painel atual e desativa o GameObject para não aparecer no editor
        // a verificação de null é necessária porque no primeiro GoTo (arranque do jogo)
        // o CurrentState é "None" e não há painel para fazer fade out
        CanvasGroup outPanel = GetPanelFor(CurrentState);
        if (outPanel != null) {
            yield return StartCoroutine(Fade(outPanel, outPanel.alpha, 0f));
            outPanel.gameObject.SetActive(false);
        }

        CurrentState = next;
        OnEnter(next);

        CanvasGroup inPanel = GetPanelFor(next);
        CanvasGroup inCard = GetCardFor(next);

        // a verificação de null é necessária porque "Playing" não tem painel de menu associado
        if (inPanel != null) {
            // ativa o painel com o card já invisível e encolhido para o BG aparecer primeiro
            inPanel.gameObject.SetActive(true);
            inPanel.alpha = 0f;
            inPanel.interactable = false;
            inPanel.blocksRaycasts = false;

            // o card também pode ser null se não tiver sido ligado no inspector
            if (inCard != null) {
                inCard.alpha = 0f;
                inCard.interactable = false;
                inCard.blocksRaycasts = false;

                // encolhe o card para o tamanho inicial da animação e o FadeAndScale fá-lo crescer de volta para Vector3.one
                inCard.transform.localScale = Vector3.one * cardStartScale;
            }

            yield return StartCoroutine(Fade(inPanel, 0f, 1f)); // Fase 1: BG faz fade in —> o background aparece
            yield return new WaitForSecondsRealtime(cardRevealDelay); // Fase 2: pausa breve para mostrar o background

            if (inCard != null)
                yield return StartCoroutine(FadeAndScale(inCard, 0f, 1f, cardStartScale, 1f)); // Fase 3: card cresce ao mesmo tempo que faz fade in
        }

        isTransitioning = false;
    }

    // chamado no momento exato em que o estado muda (antes do fade in)
    private void OnEnter(MenuState state) {
        bool isPlaying = (state == MenuState.Playing);

        Cursor.lockState = isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isPlaying;

        // a verificação de null é necessária porque o PlayerController pode não existir
        // na cena quando os menus são testados isoladamente no editor
        if (PlayerController.Instance != null)
            PlayerController.Instance.canMoveRotate = isPlaying;

        switch (state) {
            case MenuState.MainMenu:
                // "Continue" só faz sentido se existir um save
                continueButton.interactable = false;

                // o typewriter começa em OnEnter, enquanto o card ainda está a fazer
                // fade in —> as letras aparecem ao mesmo tempo que o card "materializa",
                // o que fica bem e esconde o facto de o título estar vazio no início
                PlayTypewriter();
                break;

            case MenuState.CharacterCreation:
                InitCharacterCreation();
                break;

            case MenuState.Paused:
                // dá reset aos valores dos dados
                RefreshPauseUI();
                break;
        }
    }

    private void PlayTypewriter() {
        // para a coroutine anterior se o menu principal for visitado mais do que uma vez
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

        // quando terminar fica a piscar o cursor
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
        // inicializa todos os atributos a 1 e restaura os pontos disponíveis
        statValues = new int[statRows.Length];
        for (int i = 0; i < statValues.Length; i++)
            statValues[i] = 1;

        remaining = totalExtraPoints;

        RefreshStats(false);
    }

    // FOR
    public void AddFor() {
        AdjustStat(0, +1);
    }
    public void SubFor() {
        AdjustStat(0, -1);
    }

    // PER
    public void AddPer() {
        AdjustStat(1, +1);
    }
    public void SubPer() {
        AdjustStat(1, -1);
    }

    // RES
    public void AddRes() {
        AdjustStat(2, +1);
    }
    public void SubRes() {
        AdjustStat(2, -1);
    }

    // CAR
    public void AddCar() {
        AdjustStat(3, +1);
    }
    public void SubCar() {
        AdjustStat(3, -1);
    }

    // INT
    public void AddInt() {
        AdjustStat(4, +1);
    }
    public void SubInt() {
        AdjustStat(4, -1);
    }

    // AGI
    public void AddAgi() {
        AdjustStat(5, +1);
    }
    public void SubAgi() {
        AdjustStat(5, -1);
    }

    // SOR
    public void AddSor() {
        AdjustStat(6, +1);
    }
    public void SubSor() {
        AdjustStat(6, -1);
    }

    private void AdjustStat(int idx, int delta) {
        int newValue = statValues[idx] + delta;

        // limites: mínimo 1, máximo 10  e não pode gastar pontos que não tem
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

            // desativa os botões nos limites para dar feedback visual ao jogador
            statRows[i].minusButton.interactable = (statValues[i] > 1);
            statRows[i].plusButton.interactable = (statValues[i] < 10 && remaining > 0);

            // quando o utilizador muda, aparece uma animação
            if (animated && i == changedIdx)
                StartCoroutine(AnimatePips(i));
            else
                for (int p = 0; p < statRows[i].pips.Length; p++) // pinta os pips sem animação, quando se liga o painél
                    statRows[i].pips[p].color = (p < statValues[i]) ? pipOn : pipOff;
        }
    }

    // pinta os pips sequencialmente para dar sensação de "carregar" o atributo
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

    // atualiza os dados do ecrã de pausa com os valores reais dos sistemas do jogo
    private void RefreshPauseUI() {
        // dia e hora
        int currentDay = GameManager.Instance.currentDay;
        float timeHours = TimeManager.Instance.GetCurrentTimeInHours();

        int hours = Mathf.FloorToInt(timeHours);
        int minutes = Mathf.FloorToInt((timeHours - hours) * 60f);

        pauseDayTimeText.text = $"DIA {currentDay}  //  {hours:00}:{minutes:00}";

        // nível de suspeita
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

        // bateria da lanterna
        int batteryPct = Mathf.RoundToInt(FlashlightController.Instance.GetBatteryRatio() * 100f);
        pauseBatteryText.text = $"BATERIA: {batteryPct}%";
    }

    private void WireButtons() {
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        confirmButton.onClick.AddListener(ConfirmStats);
        resetButton.onClick.AddListener(ResetStats);

        resumeButton.onClick.AddListener(ResumeGame);
        saveExitButton.onClick.AddListener(OnSaveExitClicked);
        abandonButton.onClick.AddListener(OnAbandonClicked);

        // settingsButton ainda não tem funcionalidade
    }

    // métodos para os botões, não dá para meter o GoTo diretamente porque retorna void em vez da referência do método
    private void OnNewGameClicked() {
        GoTo(MenuState.CharacterCreation);
    }

    private void OnContinueClicked() {
        GoTo(MenuState.Playing);
    }

    private void OnQuitClicked() {
        Application.Quit();
    }

    private void OnSaveExitClicked() {
        GoTo(MenuState.MainMenu);
    }

    private void OnAbandonClicked() {
        GoTo(MenuState.MainMenu);
    }

    // devolve o CanvasGroup correspondente a cada estado
    // "Playing" e "None" devolvem null porque não têm painel de menu associado
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

    // devolve o CanvasGroup do card central de cada painel usado para mostrar o BG e o card
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


    // faz fade de "from" para "to" no CanvasGroup
    // usa Time.unscaledDeltaTime para funcionar mesmo com Time.timeScale = 0 (pausa)
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

    // anima em simultâneo o alpha e o localScale do card, usa as duas animações ao mesmo tempo é o que torna a entrada gira
    // Usa Mathf.SmoothStep para ease-in-out: começa devagar, acelera no meio e desacelera no final
    // (parece mais natural do que um Lerp linear)
    private IEnumerator FadeAndScale(CanvasGroup cg, float fromAlpha, float toAlpha,
                                     float fromScale, float toScale) {
        SetPanel(cg, fromAlpha, toAlpha > 0f);

        float elapsed = 0f;
        while (elapsed < cardEntranceDuration) {
            elapsed += Time.unscaledDeltaTime;
            // t vai de 0 a 1; SmoothStep aplica ease-in-out automaticamente
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / cardEntranceDuration));

            cg.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            cg.transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, t);
            yield return null;
        }

        // para garantir que os valores finais são exatos (sem erros de floating point)
        SetPanel(cg, toAlpha, toAlpha > 0f);
        cg.transform.localScale = Vector3.one * toScale;
    }

    // aplica alpha, interactable e blocksRaycasts ao CanvasGroup de uma vez e o blocksRaycasts evita que painéis invisíveis "roubem" cliques aos painéis visíveis
    private static void SetPanel(CanvasGroup cg, float alpha, bool interactive) {
        cg.alpha = alpha;
        cg.interactable = interactive;
        cg.blocksRaycasts = interactive;
    }
}