using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour {

    [Header("Painel Filho")]
    [SerializeField] private GameObject endingPanel;         // o filho com toda a UI
    [SerializeField] private CanvasGroup endingCanvasGroup;  // CanvasGroup só para o fade, no filho
    [SerializeField] private float fadeInDuration = 1.2f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    [Header("Texto Principal")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Dia e Tempo")]
    [SerializeField] private TextMeshProUGUI txtDia;
    [SerializeField] private TextMeshProUGUI txtHora;
    [SerializeField] private TextMeshProUGUI txtFadiga;
    [SerializeField] private TextMeshProUGUI txtCafes;
    [SerializeField] private Slider fadigaBar;

    [Header("Suspeita e Empresa")]
    [SerializeField] private TextMeshProUGUI txtSuspeita;
    [SerializeField] private TextMeshProUGUI txtSuspeitaEstado;
    [SerializeField] private Slider suspeitaBar;
    [SerializeField] private TextMeshProUGUI txtCompanyAwareness;
    [SerializeField] private Slider awarenessBar;

    [Header("Intel")]
    [SerializeField] private TextMeshProUGUI txtIntelRecolhida;

    [Header("Câmaras")]
    [SerializeField] private TextMeshProUGUI txtCamerasDesbloqueadas;
    [SerializeField] private TextMeshProUGUI txtHackLevel;

    [Header("Pisos")]
    [SerializeField] private TextMeshProUGUI txtPisoAtual;
    [SerializeField] private TextMeshProUGUI txtPisosDesbloqueados;

    [Header("Objetivo Final")]
    [SerializeField] private TextMeshProUGUI txtObjFinal;

    [Header("Stats do Jogador")]
    [SerializeField] private TextMeshProUGUI txtForca;
    [SerializeField] private TextMeshProUGUI txtPercecao;
    [SerializeField] private TextMeshProUGUI txtResistencia;
    [SerializeField] private TextMeshProUGUI txtCarisma;
    [SerializeField] private TextMeshProUGUI txtIntelecto;
    [SerializeField] private TextMeshProUGUI txtAgilidade;
    [SerializeField] private TextMeshProUGUI txtSorte;

    private static readonly string[] EndingTitles = {
        "",
        "Game Over",
        "Game Over"
    };
    private static readonly string[] EndingDescriptions = {
        "",
        "Fizeste a escolha certa.",
        "Devias ter agido."
    };

    void Awake() {
        endingPanel.SetActive(false); // esconde o filho — sem CanvasGroup no root, sem blocksRaycasts
    }

    void OnEnable() { GameEvent.OnEndingReached += HandleEnding; }
    void OnDisable() { GameEvent.OnEndingReached -= HandleEnding; }

    private void HandleEnding(int ending) {
        StartCoroutine(ShowEnding(ending));
    }

    // ─── Mostrar ────────────────────────────────────────────────────────────────

    private IEnumerator ShowEnding(int ending) {
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);

        titleText.text = EndingTitles[ending];
        descriptionText.text = EndingDescriptions[ending];
        PopulateStats();

        endingPanel.SetActive(true);
        endingCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration) {
            elapsed += Time.deltaTime;
            endingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        endingCanvasGroup.alpha = 1f;
    }

    // ─── Botões do EndingPanel ───────────────────────────────────────────────────
    // Liga ESTES métodos aos botões, não os do GameMenuManager directamente

    public void OnEndingNewGame() {
        StartCoroutine(HideThen(() => GameMenuManager.Instance.OnNewGameClicked()));
    }

    public void OnEndingMainMenu() {
        StartCoroutine(HideThen(() => GameMenuManager.Instance.GoToMainMenu()));
    }

    public void OnEndingQuit() {
        GameMenuManager.Instance.OnQuitClicked(); // quit não precisa de fade
    }

    // ─── Fade out + callback ─────────────────────────────────────────────────────

    private IEnumerator HideThen(System.Action callback) {
        float elapsed = 0f;
        while (elapsed < fadeOutDuration) {
            elapsed += Time.deltaTime;
            endingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        endingPanel.SetActive(false);
        callback?.Invoke();
    }

    // ─── Populate Stats ──────────────────────────────────────────────────────────

    private void PopulateStats() {
        // Dia e Tempo
        if (txtDia != null)
            txtDia.text = $"Dia {DayManager.Instance.CurrentDay} / {DayManager.TotalDays}";

        if (txtHora != null)
            txtHora.text = TimeManager.Instance.GetTimeDisplay();

        if (txtFadiga != null || fadigaBar != null) {
            int stage = Mathf.Clamp(TimeManager.Instance.GetSleepStage(), 0, 3);
            string[] labels = { "Sem fadiga", "Leve", "Moderada", "Severa" };
            if (txtFadiga != null) txtFadiga.text = labels[stage];
            if (fadigaBar != null) fadigaBar.value = stage / 3f;
        }

        if (txtCafes != null)
            txtCafes.text = $"{TimeManager.Instance.GetCoffeesTaken()} cafés";

        // Suspeita
        if (txtSuspeita != null || suspeitaBar != null) {
            float ratio = SuspicionManager.Instance.GetSuspicionRatio();
            if (txtSuspeita != null) txtSuspeita.text = $"{ratio * 100f:F0}%";
            if (suspeitaBar != null) suspeitaBar.value = ratio;
        }

        if (txtSuspeitaEstado != null) {
            txtSuspeitaEstado.text = SuspicionManager.Instance.GetCurrentState() switch {
                SuspicionManager.SuspicionState.None => "Nenhuma",
                SuspicionManager.SuspicionState.Attention => "Atenção",
                SuspicionManager.SuspicionState.Investigation => "Investigação",
                SuspicionManager.SuspicionState.Expulsion => "Expulsão",
                _ => "-"
            };
        }

        // Company Awareness
        if (txtCompanyAwareness != null || awarenessBar != null) {
            float ratio = DocumentManager.Instance.GetCompanyAwareness();
            if (txtCompanyAwareness != null) txtCompanyAwareness.text = $"{ratio * 100f:F0}%";
            if (awarenessBar != null) awarenessBar.value = ratio;
        }

        // Intel
        if (txtIntelRecolhida != null)
            txtIntelRecolhida.text = $"{IntelInventory.Instance.GetTotalIntel()} intel";

        // Câmaras
        if (txtCamerasDesbloqueadas != null) {
            bool[] unlocked = CameraSystem.Instance.cameraUnlocked;
            int count = 0;
            if (unlocked != null) foreach (bool b in unlocked) if (b) count++;
            int total = unlocked?.Length ?? 0;
            txtCamerasDesbloqueadas.text = $"{count} / {total}";
        }

        if (txtHackLevel != null)
            txtHackLevel.text = $"nível hack {CameraHackPuzzle.HackLevel}";

        // Pisos
        if (txtPisoAtual != null)
            txtPisoAtual.text = $"piso atual: {GameManager.Instance.currentFloor}";

        if (txtPisosDesbloqueados != null) {
            bool[] floors = GameManager.Instance.GetFloorsUnlocked();
            int count = 0;
            if (floors != null) foreach (bool b in floors) if (b) count++;
            int total = floors?.Length ?? 0;
            txtPisosDesbloqueados.text = $"{count} / {total}";
        }

        // Objetivo Final
        if (txtObjFinal != null)
            txtObjFinal.text = DayManager.Instance.finalObjectiveCompleted ? "Concluído" : "Falhado";

        // Stats do Jogador
        if (PlayerStats.Instance == null) return;
        if (txtForca != null) txtForca.text = PlayerStats.Instance.GetForca().ToString();
        if (txtPercecao != null) txtPercecao.text = PlayerStats.Instance.GetPercecao().ToString();
        if (txtResistencia != null) txtResistencia.text = PlayerStats.Instance.GetResistencia().ToString();
        if (txtCarisma != null) txtCarisma.text = PlayerStats.Instance.GetCarisma().ToString();
        if (txtIntelecto != null) txtIntelecto.text = PlayerStats.Instance.GetIntelecto().ToString();
        if (txtAgilidade != null) txtAgilidade.text = PlayerStats.Instance.GetAgilidade().ToString();
        if (txtSorte != null) txtSorte.text = PlayerStats.Instance.GetSorte().ToString();
    }
}