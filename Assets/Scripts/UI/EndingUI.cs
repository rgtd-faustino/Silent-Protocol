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
    [SerializeField] private TextMeshProUGUI txtPercentagemFinal;

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

    // índices: 1=relatório bom, 2=relatório mau, 3=apanhado pela suspeita, 4=exaustão (sono severo)
    private static readonly string[] EndingTitles = {
        "",
        "Game Over",
        "Game Over",
        "Game Over",
        "Game Over"
    };
    private static readonly string[] EndingDescriptions = {
        "",
        "O teu agente recebeu o relatório. A empressa foi precessada e os dados não foram vendidos. Salvas-te a privacidade de milhões.",
        "A informação do relatóio não foi suficiente para encriminar a empressa. Perdes-te o emprego e os dados foram vendidos.",
        "Foste descoberto. A segurança apanhou-te antes que pudesses escapar.",
        "O cansaço venceu-te. Da próxima vez, regula melhor os teus horários de sono."
    };

    void Awake() {
        endingPanel.SetActive(false); // esconde o filho — sem CanvasGroup no root, sem blocksRaycasts
    }

    void OnEnable() { GameEvent.OnEndingReached += HandleEnding; }
    void OnDisable() { GameEvent.OnEndingReached -= HandleEnding; }

    private void HandleEnding(int ending) {
        StartCoroutine(ShowEnding(ending));
    }


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


    public void OnEndingNewGame() {
        StartCoroutine(HideThen(() => GameMenuManager.Instance.OnNewGameClicked()));
    }

    public void OnEndingMainMenu() {
        StartCoroutine(HideThen(() => GameMenuManager.Instance.GoToMainMenu()));
    }

    public void OnEndingQuit() {
        GameMenuManager.Instance.OnQuitClicked(); // quit não precisa de fade
    }

    // Fade out + callback

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

    // Populate Stats

    private void PopulateStats() {
        // Dia e Tempo
        txtDia.text = $"Dia {GameManager.Instance.currentDay} / {GameManager.TotalDays}";

        txtHora.text = TimeManager.Instance.GetTimeDisplay();

        int stage = Mathf.Clamp(TimeManager.Instance.GetSleepStage(), 0, 3);
        string[] labels = { "Sem fadiga", "Leve", "Moderada", "Severa" };
        txtFadiga.text = labels[stage];
        fadigaBar.value = stage / 3f;

        txtCafes.text = $"{TimeManager.Instance.GetCoffeesTaken()} cafés";

        // Suspeita
        float ratio = SuspicionManager.Instance.GetSuspicionRatio();
        txtSuspeita.text = $"{ratio * 100f:F0}%";
        suspeitaBar.value = ratio;

        txtSuspeitaEstado.text = SuspicionManager.Instance.GetCurrentState() switch {
            SuspicionManager.SuspicionState.None => "Nenhuma",
            SuspicionManager.SuspicionState.Attention => "Atenção",
            SuspicionManager.SuspicionState.Investigation => "Investigação",
            SuspicionManager.SuspicionState.Expulsion => "Expulsão",
            _ => "-"
        };

        // Company Awareness
        float ratio2 = DocumentManager.Instance.GetCompanyAwareness();
        txtCompanyAwareness.text = $"{ratio2 * 100f:F0}%";
        awarenessBar.value = ratio2;

        // Intel
        txtIntelRecolhida.text = $"{IntelInventory.Instance.GetTotalIntel()} intel";

        txtPercentagemFinal.text = $"{IntelInventory.Instance.GetTotalPercentage():F0}%";

        // Câmaras
        bool[] unlocked = CameraSystem.Instance.cameraUnlocked;
        int count = 0;
        if (unlocked != null) foreach (bool b in unlocked) if (b) count++;
        int total = unlocked?.Length ?? 0;
        txtCamerasDesbloqueadas.text = $"{count} / {total}";

        txtHackLevel.text = $"nível hack {CameraHackPuzzle.HackLevel}";

        // Pisos
        txtPisoAtual.text = $"piso atual: {GameManager.Instance.currentFloor}";

        bool[] floors = GameManager.Instance.GetFloorsUnlocked();
        int count2 = 0;
        if (floors != null) foreach (bool b in floors) if (b) count2++;
        int total2 = floors?.Length ?? 0;
        txtPisosDesbloqueados.text = $"{count2} / {total2}";


        // Stats do Jogador
        txtForca.text = PlayerStats.Instance.GetForca().ToString();
        txtPercecao.text = PlayerStats.Instance.GetPercecao().ToString();
        txtResistencia.text = PlayerStats.Instance.GetResistencia().ToString();
        txtCarisma.text = PlayerStats.Instance.GetCarisma().ToString();
        txtIntelecto.text = PlayerStats.Instance.GetIntelecto().ToString();
        txtAgilidade.text = PlayerStats.Instance.GetAgilidade().ToString();
        txtSorte.text = PlayerStats.Instance.GetSorte().ToString();
    }
}