using UnityEngine;

public class TaskManager : MonoBehaviour {
    public static TaskManager Instance;

    [Header("UI das tasks")]
    [SerializeField] private TaskItemUI morningTaskUI;
    [SerializeField] private TaskItemUI afternoonTaskUI;

    [Header("Impressoras disponíveis para a task de imprimir")]
    public GameObject printerList;

    private const float LunchStart = 720f;
    private const float AfternoonStart = 780f;
    private const float DinnerStart = 1140f;

    private string[] morningOptions = { "Escrever documento", "Imprimir documento" };
    //private string[] afternoonOptions = { "Arquivar documento", "Entregar documento" };

    public enum TaskDifficulty { Small, Medium, Major }

    private class TaskEntry {
        public string name;
        public float deadlineMinutes;
        public TaskDifficulty difficulty;
        public TaskItemUI ui;
        public bool completed;
        public bool failed;
    }

    private TaskEntry morningTask;
    private TaskEntry afternoonTask;
    private bool afternoonSpawned = false;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;

    }

    void OnEnable() {
        // subscrevemo nos aos eventos relevantes
        GameEvent.OnWorkHoursStarted += ActivateTasks;
        GameEvent.OnAfternoonStarted += SpawnAfternoonTask;
    }

    // para não haver fugas de memória ou problemas desse género
    void OnDisable() {
        GameEvent.OnWorkHoursStarted -= ActivateTasks;
        GameEvent.OnAfternoonStarted -= SpawnAfternoonTask;
    }

    void Update() {
        float now = TimeManager.Instance.GetCurrentTimeInHours() * 60f;
        CheckDeadline(morningTask, now);
        CheckDeadline(afternoonTask, now);
    }

    private void ActivateTasks() {
        float now = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

        morningTaskUI.gameObject.SetActive(false);
        afternoonTaskUI.gameObject.SetActive(false);

        SpawnTask(morningOptions, now, LunchStart, ref morningTask, morningTaskUI, TaskDifficulty.Medium);
        afternoonSpawned = false;
    }

    private void SpawnAfternoonTask() {
        if (afternoonSpawned) 
            return;

        // a task da tarde depende diretamente da manhã:
        // Escrever -> Entregar
        // Imprimir -> Arquivar
        string afternoonName;
        switch (morningTask.name) {
            case "Escrever documento":
                afternoonName = "Entregar documento";
                break;

            case "Imprimir documento":
                afternoonName = "Arquivar documento";
                break;

            default:
                afternoonName = "Não vai acontecer";
                break;
        }


        SpawnTask(new[] { afternoonName }, AfternoonStart, DinnerStart, ref afternoonTask, afternoonTaskUI, TaskDifficulty.Medium);
        afternoonSpawned = true;
    }

    private void SpawnTask(string[] options, float periodStart, float periodEnd, ref TaskEntry entry, TaskItemUI ui, TaskDifficulty difficulty) {
        string name = options[Random.Range(0, options.Length)];

        float window = periodEnd - periodStart;
        float deadlineMin = periodStart + window * 0.70f;
        float deadlineMax = periodEnd - 15f;
        float deadline = Random.Range(deadlineMin, deadlineMax);

        entry = new TaskEntry {
            name = name,
            deadlineMinutes = deadline,
            difficulty = difficulty,
            ui = ui
        };

        int h = (int)(deadline / 60f);
        int m = (int)(deadline % 60f);
        ui.SetTask(name, $"{h:00}:{m:00}");

        Debug.Log($"[TaskManager] Task criada: '{name}' | Deadline: {h:00}:{m:00}");
    }

    private void CheckDeadline(TaskEntry task, float now) {
        if (task == null || task.completed || task.failed) return;
        if (now < task.deadlineMinutes) return;

        task.failed = true;
        task.ui.SetFailed();
        HandleTaskComplete(task.difficulty, false);
        Debug.Log($"[TaskManager] Task '{task.name}' falhada.");
    }

    public void CompleteTask(string taskName, bool doneCorrectly) {
        TaskEntry task = null;

        if (morningTask != null && morningTask.name == taskName && !morningTask.completed && !morningTask.failed)
            task = morningTask;
        else if (afternoonTask != null && afternoonTask.name == taskName && !afternoonTask.completed && !afternoonTask.failed)
            task = afternoonTask;

        if (task == null)
            return;

        task.completed = true;
        task.ui.SetCompleted();
        HandleTaskComplete(task.difficulty, doneCorrectly);
        Debug.Log($"[TaskManager] Task '{taskName}' completada. Correto: {doneCorrectly}");
    }

    private void HandleTaskComplete(TaskDifficulty difficulty, bool doneCorrectly) {
        float multiplier;

        switch (difficulty) {
            case TaskDifficulty.Small:
                multiplier = 0.1f;
                break;

            case TaskDifficulty.Medium:
                multiplier = 0.25f;
                break;

            case TaskDifficulty.Major:
                multiplier = 0.5f;
                break;

            default:
                multiplier = 0f;
                break;
        }

        SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(multiplier, doneCorrectly);
    }

    public void ActivatePrinterTask() {
        int index = Random.Range(0, printerList.transform.childCount);
        printerList.transform.GetChild(index).gameObject.GetComponent<ImpressoraScript>().ActivatePrinterTask();
    }

    public bool HasActiveMorningTask(string name) {
        if (morningTask == null) 
            return false;

        if (morningTask.name != name) 
            return false;

        if (morningTask.completed) 
            return false;

        if (morningTask.failed) 
            return false;

        return true;
    }

    public bool HasActiveAfternoonTask(string name) {
        if (afternoonTask == null) 
            return false;

        if (afternoonTask.name != name) 
            return false;

        if (afternoonTask.completed) 
            return false;

        if (afternoonTask.failed) 
            return false;

        return true;
    }
}