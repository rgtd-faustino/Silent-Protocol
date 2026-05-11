using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // ---------------------------------------------------------------
    // Prefab de UI — vai ser instanciado uma vez por task
    // Arrasta aqui o teu prefab de TaskItemUI no Inspector
    // ---------------------------------------------------------------
    [Header("Prefab de UI para cada task")]
    [SerializeField] private TaskItemUI taskItemUIPrefab;

    // Container onde os TaskItemUI instanciados vão aparecer no ecrã
    // (um VerticalLayoutGroup funciona bem aqui)
    [SerializeField] private Transform taskListContainer;

    [Header("Impressoras disponíveis para a task de imprimir")]
    public GameObject printerList;

    // ---------------------------------------------------------------
    // HORÁRIOS POR DIA
    // daySchedules[0] = Dia 1, daySchedules[1] = Dia 2, etc.
    // ---------------------------------------------------------------
    [Header("Horários  (um DaySchedule asset por dia, por ordem)")]
    [SerializeField] private DaySchedule[] daySchedules;

    public enum TaskDifficulty { Small, Medium, Major }

    // ---------------------------------------------------------------
    // Estrutura interna de cada task ativa
    // ---------------------------------------------------------------
    private class TaskEntry
    {
        public string name;
        public float spawnMinutes;
        public float deadlineMinutes;
        public TaskDifficulty difficulty;
        public TaskItemUI ui;
        public bool spawned;
        public bool completed;
        public bool failed;
    }

    // lista de todas as tasks do dia (sem limite de quantidade)
    private List<TaskEntry> activeTasks = new List<TaskEntry>();

    // ---------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable() => GameEvent.OnWorkHoursStarted += ActivateTasks;
    void OnDisable() => GameEvent.OnWorkHoursStarted -= ActivateTasks;

    void Update()
    {
        float now = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

        foreach (TaskEntry task in activeTasks)
        {
            TrySpawnTask(task, now);
            CheckDeadline(task, now);
        }
    }

    // ---------------------------------------------------------------
    // Carrega o schedule do dia e prepara todas as TaskEntry
    // ---------------------------------------------------------------
    private void ActivateTasks()
    {
        // limpa tasks e UI do dia anterior
        foreach (TaskEntry t in activeTasks)
        {
            if (t.ui != null) Destroy(t.ui.gameObject);
        }
        activeTasks.Clear();

        DaySchedule schedule = GetScheduleForToday();

        if (schedule == null || schedule.tasks == null || schedule.tasks.Length == 0)
        {
            Debug.LogWarning($"[TaskManager] Nenhum DaySchedule com tasks encontrado para o dia {GameManager.Instance.currentDay}.");
            return;
        }

        foreach (DaySchedule.ScheduledTask scheduled in schedule.tasks)
        {
            if (string.IsNullOrEmpty(scheduled.taskName)) continue;

            // valida horas
            if (scheduled.deadlineHour <= scheduled.spawnHour)
            {
                Debug.LogWarning($"[TaskManager] Task '{scheduled.taskName}': deadlineHour ({scheduled.deadlineHour}) tem de ser maior que spawnHour ({scheduled.spawnHour}). Task ignorada.");
                continue;
            }

            // instancia um TaskItemUI para esta task (começa escondido)
            TaskItemUI ui = Instantiate(taskItemUIPrefab, taskListContainer);
            ui.gameObject.SetActive(false);

            TaskEntry entry = new TaskEntry
            {
                name = scheduled.taskName,
                spawnMinutes = scheduled.spawnHour * 60f,
                deadlineMinutes = scheduled.deadlineHour * 60f,
                difficulty = scheduled.difficulty,
                ui = ui,
                spawned = false,
                completed = false,
                failed = false
            };

            activeTasks.Add(entry);
        }

        Debug.Log($"[TaskManager] Dia {GameManager.Instance.currentDay}: {activeTasks.Count} task(s) preparada(s).");
    }

    // ---------------------------------------------------------------
    // Mostra a task na UI quando chega a hora de spawn
    // ---------------------------------------------------------------
    private void TrySpawnTask(TaskEntry task, float now)
    {
        if (task.spawned || task.failed) return;
        if (now < task.spawnMinutes) return;

        task.spawned = true;
        task.ui.gameObject.SetActive(true);

        int dh = (int)(task.deadlineMinutes / 60f);
        int dm = (int)(task.deadlineMinutes % 60f);
        task.ui.SetTask(task.name, $"{dh:00}:{dm:00}");

        Debug.Log($"[TaskManager] Task '{task.name}' apareceu. Deadline: {dh:00}:{dm:00}");
    }

    // ---------------------------------------------------------------
    // Verifica se a deadline passou sem a task ter sido completada
    // ---------------------------------------------------------------
    private void CheckDeadline(TaskEntry task, float now)
    {
        if (!task.spawned || task.completed || task.failed) return;
        if (now < task.deadlineMinutes) return;

        task.failed = true;
        task.ui.SetFailed();
        HandleTaskComplete(task.difficulty, false);
        Debug.Log($"[TaskManager] Task '{task.name}' falhada por tempo.");
    }

    // ---------------------------------------------------------------
    // Chamado pelos objetos do mundo (ArchiveScript, ImpressoraScript, etc.)
    // ---------------------------------------------------------------
    public void CompleteTask(string taskName, bool doneCorrectly)
    {
        // procura a primeira task ativa com esse nome (spawned, não completada, não falhada)
        TaskEntry task = activeTasks.Find(t =>
            t.name == taskName &&
            t.spawned &&
            !t.completed &&
            !t.failed
        );

        if (task == null)
        {
            Debug.LogWarning($"[TaskManager] Tentativa de completar '{taskName}' mas não há task ativa com esse nome.");
            return;
        }

        task.completed = true;
        task.ui.SetCompleted();
        HandleTaskComplete(task.difficulty, doneCorrectly);
        Debug.Log($"[TaskManager] Task '{taskName}' completada. Correto: {doneCorrectly}");
    }

    private void HandleTaskComplete(TaskDifficulty difficulty, bool doneCorrectly)
    {
        float multiplier = difficulty switch
        {
            TaskDifficulty.Small => 0.1f,
            TaskDifficulty.Medium => 0.25f,
            TaskDifficulty.Major => 0.5f,
            _ => 0f
        };
        SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(multiplier, doneCorrectly);
    }

    // ---------------------------------------------------------------
    // Ativa uma impressora aleatória para a task de imprimir
    // ---------------------------------------------------------------
    public ImpressoraScript ActivatePrinterTask()
    {
        int index = Random.Range(0, printerList.transform.childCount);
        ImpressoraScript printer = printerList.transform.GetChild(index).GetComponent<ImpressoraScript>();
        printer.ActivatePrinterTask();
        return printer;
    }

    // ---------------------------------------------------------------
    // Helpers usados por WriteDocumentUI, UIManager, etc.
    // ---------------------------------------------------------------

    // devolve true se existir pelo menos uma task ativa (spawned, não terminada) com esse nome
    public bool HasActiveTask(string name)
    {
        return activeTasks.Exists(t =>
            t.name == name &&
            t.spawned &&
            !t.completed &&
            !t.failed
        );
    }

    // mantidos por compatibilidade com WriteDocumentUI e UIManager existentes
    public bool HasActiveMorningTask(string name) => HasActiveTask(name);
    public bool HasActiveAfternoonTask(string name) => HasActiveTask(name);

    // ---------------------------------------------------------------
    // Seleciona o DaySchedule correto para o dia atual
    // ---------------------------------------------------------------
    private DaySchedule GetScheduleForToday()
    {
        int index = GameManager.Instance.currentDay - 1;
        if (daySchedules == null || index < 0 || index >= daySchedules.Length) return null;
        return daySchedules[index];
    }
}   