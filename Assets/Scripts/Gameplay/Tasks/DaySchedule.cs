using UnityEngine;

// Como usar:
// Boto direito no Project -> Create -> Tasks -> Day Schedule
// Cria um asset por dia (Day1Schedule, Day2Schedule, etc.)
// Em "Tasks Do Dia" clica no "+" para adicionar quantas tasks quiseres

[CreateAssetMenu(menuName = "Tasks/Day Schedule")]
public class DaySchedule : ScriptableObject
{

    [System.Serializable]
    public class ScheduledTask
    {

        [Tooltip("Nome exato da task.\nOpes: 'Escrever documento' | 'Imprimir documento' | 'Arquivar documento' | 'Entregar documento'")]
        public string taskName;

        [Tooltip("Hora a que a task aparece para o jogador.\nExemplos: 9 = 09:00 | 13.5 = 13:30 | 14.25 = 14:15")]
        [Range(0f, 23.99f)]
        public float spawnHour;

        [Tooltip("Hora limite para completar a task.\nTem de ser maior que spawnHour.")]
        [Range(0f, 23.99f)]
        public float deadlineHour;

        [Tooltip("Impacto na suspeita ao falhar ou completar.\nSmall = pouco | Medium = mdio | Major = muito")]
        public TaskManager.TaskDifficulty difficulty;
    }

    [Header("Tasks do Dia  (adiciona quantas quiseres com o '+')")]
    public ScheduledTask[] tasks;
}