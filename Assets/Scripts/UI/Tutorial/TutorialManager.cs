using UnityEngine;

public class TutorialManager : MonoBehaviour {
    public static TutorialManager Instance;

    [System.Serializable]
    public class TutorialStep {
        [TextArea(2, 4)]
        public string message;
        public Transform target;
        public Vector2 screenOffset = new Vector2(70, 40);

        [Tooltip("Se marcado, o passo avança quando o jogador prime E sobre o popup. Se desmarcado, só avança quando outro script chamar CompleteCurrentStep().")]
        public bool dismissWithE = true;

        [Tooltip("Identificador usado por outros scripts para confirmarem que é este o passo à espera deles.")]
        public string gateId;
    }

    [SerializeField] private TutorialFeedPrompt promptPrefab;
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private TutorialStep[] steps;

    private int currentIndex = -1;
    private TutorialFeedPrompt activeInstance;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update() {
        if (activeInstance == null || currentIndex < 0 || currentIndex >= steps.Length) return;
        if (steps[currentIndex].dismissWithE && Input.GetKeyDown(KeyCode.E))
            CompleteCurrentStep();
    }

    public void StartTutorial() {
        currentIndex = -1;
        ShowNextStep();
    }

    public bool IsCurrentStepGate(string id) {
        return currentIndex >= 0 && currentIndex < steps.Length && steps[currentIndex].gateId == id;
    }

    // chamado a partir de outros scripts (NPCScript, IntelPickup, etc.)
    public void CompleteCurrentStep() {
        if (activeInstance != null) activeInstance.Dismiss();
        activeInstance = null;
        ShowNextStep();
    }

    private void ShowNextStep() {
        currentIndex++;
        if (currentIndex >= steps.Length) return;

        TutorialStep step = steps[currentIndex];
        activeInstance = Instantiate(promptPrefab, tutorialCanvas.transform);
        activeInstance.SetCanvas(tutorialCanvas);
        activeInstance.Show(step.message, step.target, step.screenOffset, step.dismissWithE);
    }
}
