using UnityEngine;

public class TaskManager : MonoBehaviour {
    public static TaskManager Instance;

    [Header("Task UI Slots")]
    [SerializeField] private TaskItemUI morningTaskUI;
    [SerializeField] private TaskItemUI afternoonTaskUI;

    // Limites de tempo em minutos
    private const float LunchStart = 720f;  // 12:00
    private const float AfternoonStart = 780f;  // 13:00
    private const float DinnerStart = 1140f; // 19:00

    private readonly string[] morningOptions = { "Escrever documento", "Imprimir documento" };
    private readonly string[] afternoonOptions = { "Arquivar documento", "Entregar documento" };

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
    private bool afternoonSpawned;


    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update() {
        float now = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

        // Spawn da task da tarde quando o almoço acaba
        if (!afternoonSpawned && now >= AfternoonStart) {
            SpawnTask(afternoonOptions, AfternoonStart, DinnerStart, ref afternoonTask, afternoonTaskUI, TaskDifficulty.Medium);
            afternoonSpawned = true;
        }

        CheckDeadline(morningTask, now);
        CheckDeadline(afternoonTask, now);
    }

    // Chamado pelo TimeManager quando começa o trabalho
    public void ActivateTasks() {
        float now = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

        morningTaskUI.gameObject.SetActive(false);
        afternoonTaskUI.gameObject.SetActive(false);

        SpawnTask(morningOptions, now, LunchStart, ref morningTask, morningTaskUI, TaskDifficulty.Medium);

        afternoonSpawned = false;
    }


    private void SpawnTask(string[] options, float periodStart, float periodEnd,
                           ref TaskEntry entry, TaskItemUI ui, TaskDifficulty difficulty) {
        string name = options[Random.Range(0, options.Length)];

        // Deadline nos últimos 30 % do período, com 15 min de folga no fim
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
        OnTaskComplete(task.difficulty, false);
        Debug.Log($"[TaskManager] Task '{task.name}' falhada (passou o deadline).");
    }

    // Chama isto quando o jogador completar uma task (ex: no script de interação)
    public void CompleteTask(string taskName, bool doneCorrectly) {
        TaskEntry task = null;

        if (morningTask != null && morningTask.name == taskName && !morningTask.completed && !morningTask.failed)
            task = morningTask;

        else if (afternoonTask != null && afternoonTask.name == taskName && !afternoonTask.completed && !afternoonTask.failed)
            task = afternoonTask;

        if (task == null) return;

        task.completed = true;
        task.ui.SetCompleted();
        OnTaskComplete(task.difficulty, doneCorrectly);
        Debug.Log($"[TaskManager] Task '{taskName}' completada. Correto: {doneCorrectly}");
    }

    public void OnTaskComplete(TaskDifficulty difficulty, bool doneCorrectly) {
        float multiplier = 0;
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

        }
        SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(multiplier, doneCorrectly);
    }

    // Getters úteis para scripts de interação
    public bool HasActiveMorningTask(string name) =>
        morningTask != null && morningTask.name == name && !morningTask.completed && !morningTask.failed;

    public bool HasActiveAfternoonTask(string name) =>
        afternoonTask != null && afternoonTask.name == name && !afternoonTask.completed && !afternoonTask.failed;
}