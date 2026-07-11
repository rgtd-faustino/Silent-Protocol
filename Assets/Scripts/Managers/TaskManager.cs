using System.Collections.Generic;
using UnityEngine;

// #my_code - Atribuição e verificação de tarefas diárias ao jogador para manter aparência corporativa
public class TaskManager : MonoBehaviour {
    public static TaskManager Instance;

    [Header("Prefab de UI para cada task")]
    [SerializeField] private TaskItemUI taskItemUIPrefab;

    [SerializeField] private Transform taskListContainer; // transform onde ficam as instâncias de UI de cada task ativa

    [Header("Impressoras disponíveis para a task de imprimir")]
    public GameObject printerList;

    [Header("Horários  (um DaySchedule asset por dia, por ordem)")]
    [SerializeField] private DaySchedule[] daySchedules;

    public enum TaskDifficulty {
        Small, // falhar sobe pouco a suspeita - 10%
        Medium, // 25%
        Major // 50%
    }

    // esta estrutura junta os dados vindos do ScriptableObject (DaySchedule) com o estado que muda no runtime
    // assim não precisamos de modificar os assets que são lidos e garantimos que as falhas e sucessos das tarefas ficam limpos para serializar
    private class TaskEntry {
        public string name;
        public float spawnMinutes;
        public float deadlineMinutes;
        public TaskDifficulty difficulty;
        public TaskItemUI ui;
        public bool spawned;
        public bool completed;
        public bool failed;
    }

    // lista das tasks do dia atual, reconstruída de raiz sempre que o ActivateTasks corre no início do horário de trabalho
    private List<TaskEntry> activeTasks = new List<TaskEntry>();

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable() => GameEvent.OnWorkHoursStarted += ActivateTasks;
    void OnDisable() => GameEvent.OnWorkHoursStarted -= ActivateTasks;

    void Update() {
        float now = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

        // percorremos as tasks todas a cada frame para dar spawn e verificar deadlines em tempo real
        foreach (TaskEntry task in activeTasks) {
            TrySpawnTask(task, now);
            CheckDeadline(task, now);
        }
    }

    // ligámos ao evento OnWorkHoursStarted para inicializar o dia
    // destruímos as instâncias de UI do dia anterior e carregamos o schedule novo, se não fizéssemos isto iríamos criar memory leaks com os prefabs antigos empilhados
    private void ActivateTasks() {
        foreach (TaskEntry t in activeTasks) {
            if (t.ui != null) Destroy(t.ui.gameObject);
        }
        activeTasks.Clear();

        DaySchedule schedule = GetScheduleForToday();

        if (schedule == null || schedule.tasks == null || schedule.tasks.Length == 0) {
            Debug.LogWarning($"[TaskManager] Nenhum DaySchedule com tasks encontrado para o dia {GameManager.Instance.currentDay}.");
            return;
        }

        foreach (DaySchedule.ScheduledTask scheduled in schedule.tasks) {
            if (string.IsNullOrEmpty(scheduled.taskName)) continue;

            if (scheduled.deadlineHour <= scheduled.spawnHour) {
                Debug.LogWarning($"[TaskManager] Task '{scheduled.taskName}': deadlineHour ({scheduled.deadlineHour}) tem de ser maior que spawnHour ({scheduled.spawnHour}). Task ignorada.");
                continue;
            }

            TaskItemUI ui = Instantiate(taskItemUIPrefab, taskListContainer);
            ui.gameObject.SetActive(false); // só fica visível mais tarde, quando o TrySpawnTask ativar a task à hora certa

            TaskEntry entry = new TaskEntry {
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

    // ativa a task (mostra a UI) só quando o relógio chega ao spawnMinutes definido no schedule e só uma vez por task (daí a flag spawned)
    private void TrySpawnTask(TaskEntry task, float now) {
        if (task.spawned || task.failed) return;
        if (now < task.spawnMinutes) return;

        task.spawned = true;
        task.ui.gameObject.SetActive(true);

        int dh = (int)(task.deadlineMinutes / 60f);
        int dm = (int)(task.deadlineMinutes % 60f);
        task.ui.SetTask(task.name, $"{dh:00}:{dm:00}");

        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.taskAppeared);

        Debug.Log($"[TaskManager] Task '{task.name}' apareceu. Deadline: {dh:00}:{dm:00}");
    }

    // se a task já apareceu mas ninguém a completou até à deadline, marcamos como falhada automaticamente e aplicamos a penalização
    private void CheckDeadline(TaskEntry task, float now) {
        if (!task.spawned || task.completed || task.failed) return;
        if (now < task.deadlineMinutes) return;

        task.failed = true;
        task.ui.SetFailed();
        HandleTaskComplete(task.difficulty, false);
        Debug.Log($"[TaskManager] Task '{task.name}' falhada por tempo.");
    }

    // chamado sempre que o jogador termina a ação de gameplay ligada a uma task (ex: imprimir um documento, arquivar um papel)
    // procuramos na lista por nome em vez de guardar uma referência direta para o script de gameplay não precisar de apanhar e passar a TaskEntry
    public void CompleteTask(string taskName, bool doneCorrectly) {
        TaskEntry task = activeTasks.Find(t =>
            t.name == taskName &&
            t.spawned &&
            !t.completed &&
            !t.failed
        );

        if (task == null) {
            Debug.LogWarning($"[TaskManager] Tentativa de completar '{taskName}' mas não há task ativa com esse nome.");
            return;
        }

        task.completed = true;
        task.ui.SetCompleted();
        HandleTaskComplete(task.difficulty, doneCorrectly);
        Debug.Log($"[TaskManager] Task '{taskName}' completada. Correto: {doneCorrectly}");
    }

    // decidimos usar valores percentuais empíricos porque as falhas devem ser difíceis de ignorar
    // mas dar um gameover direto seria demasiado frustrante, envia a penalização para a barra do SuspicionManager
    private void HandleTaskComplete(TaskDifficulty difficulty, bool doneCorrectly) {
        float multiplier = difficulty switch {
            TaskDifficulty.Small => 0.1f,
            TaskDifficulty.Medium => 0.25f,
            TaskDifficulty.Major => 0.5f,
            _ => 0f
        };
        SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(multiplier, doneCorrectly);

        if (doneCorrectly)
            SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.buzzerCorrect);
        else
            SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.buzzerWrong);
    }

    // escolhe uma impressora ao calhas de entre as disponíveis na cena e ativa a task nela
    public ImpressoraScript ActivatePrinterTask() {
        int index = Random.Range(0, printerList.transform.childCount);
        ImpressoraScript printer = printerList.transform.GetChild(index).GetComponent<ImpressoraScript>();
        printer.ActivatePrinterTask();
        return printer;
    }

    public bool HasActiveTask(string name) {
        return activeTasks.Exists(t =>
            t.name == name &&
            t.spawned &&
            !t.completed &&
            !t.failed
        );
    }

    public bool HasActiveMorningTask(string name) => HasActiveTask(name);
    public bool HasActiveAfternoonTask(string name) => HasActiveTask(name);

    // os DaySchedule ficam por ordem no array, por isso o dia atual (1-based) mapeia diretamente para o índice (0-based)
    private DaySchedule GetScheduleForToday() {
        int index = GameManager.Instance.currentDay - 1;
        if (daySchedules == null || index < 0 || index >= daySchedules.Length) return null;
        return daySchedules[index];
    }
}