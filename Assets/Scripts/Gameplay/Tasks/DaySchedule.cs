using UnityEngine;

[CreateAssetMenu(menuName = "Tasks/Day Schedule")]
public class DaySchedule : ScriptableObject
{

    [System.Serializable]
    public class ScheduledTask
    {
        [Tooltip("Nome exato da task. Opções: Escrever documento, Imprimir documento, Arquivar documento, Entregar documento")]
        public string taskName;

        // O TaskManager pega neste valor decimal e multiplica por 60 para saber os minutos reais do jogo e cruzar com o TimeManager.
        [Tooltip("Hora a que a task aparece (decimal). Exemplo: 13.5 corresponde às 13:30.")]
        [Range(0f, 23.99f)]
        public float spawnHour;

        [Tooltip("Hora limite para a task. Tem de ser maior do que a spawnHour.")]
        [Range(0f, 23.99f)]
        public float deadlineHour;

        // Passamos este nível de dificuldade ao TaskManager, que aplica os multiplicadores 0.1, 0.25 ou 0.5 e reencaminha para o SuspicionManager.
        [Tooltip("Define o impacto na suspeita. Small penaliza pouco, Major penaliza muito.")]
        public TaskManager.TaskDifficulty difficulty;
    }

    [Header("Tasks do Dia")]
    public ScheduledTask[] tasks;
}