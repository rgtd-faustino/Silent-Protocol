using System;

// #my_code - Sistema de eventos desacoplados (publisher/subscriber) que substitui polling de booleans
// conjunto de eventos do jogo, em vez de scripts chamarem uns aos outros diretamente, disparam eventos aqui e quem precisar de reagir subscreve
public static class GameEvent
{

    // ciclo do dia
    public static event Action<int> OnDayChanged; // dia 1-5
    public static event Action OnWorkHoursStarted; // tasks do dia devem aparecer
    public static event Action OnNightStarted;
    public static event Action OnDayEnded; // game manager subscreve se a este evento para guardar progresso

    // estado do jogo
    public static event Action OnGameOver; // game manager subscreve se ao evento para determinar quando o jogo acaba devido ao jogador ser apanhado pela suspeita
    public static event Action<int> OnEndingReached; // 1 = relatório bom, 2 = relatório mau, 3 = apanhado pela suspeita, 4 = exaustão (sono severo)
    public static event Action<int> OnDayStarted;
    private static event Action OnPlayerExhausted; // sono acumulado atingiu o estágio severo (3)

    // Reunião - evento onde todos os NPC se juntam para a sala de reuniões que o jogador pode tentar ouvir para apanhar intel
    public static event Action OnMeetingStarted;

    // Suspeita
    public static event Action<SuspicionManager.SuspicionState> OnSuspicionStateChanged;

    // Email Crítico
    private static event Action<string> OnCriticalEmailAvailable;
    public static event Action<string> OnCriticalEmailExpired;




    // o card credential pickup subscreve-se a este evento para saber quando deve mostrar os cartões de acesso que desbloqueiam determinadas portas
    public static void DayChanged(int day) => OnDayChanged?.Invoke(day); // o GameManager é o script que atualiza quando o dia muda
    public static void DayStarted(int day) => OnDayStarted?.Invoke(day); // o GameManager também indica quando o dia começa
    public static void EndingReached(int ending) => OnEndingReached?.Invoke(ending); // o GameManager também indica quando o jogador chegou ao fim
    public static void WorkHoursStarted() => OnWorkHoursStarted?.Invoke();
    public static void NightStarted() => OnNightStarted?.Invoke(); // o flashLightController usa este evento para dar reset à bateria da lanterna
    public static void DayEnded() => OnDayEnded?.Invoke(); // timeManager invoca isto para dizer quando o dia troca ou quando o jogador vai dormir
    public static void GameOver() => OnGameOver?.Invoke(); // o suspicionManager invoca o evento quando a suspeita chega ao último estado (expulsão) que depois chama o game over do gameManager
    public static void SuspicionStateChanged(SuspicionManager.SuspicionState s) => OnSuspicionStateChanged?.Invoke(s);
    public static void PlayerExhausted() => OnPlayerExhausted?.Invoke(); // timeManager invoca o evento quando o sono acumulado do jogador atinge o estado mais severo
    public static void MeetingStarted() => OnMeetingStarted?.Invoke();
    public static void CriticalEmailAvailable(string id) => OnCriticalEmailAvailable?.Invoke(id);
    public static void CriticalEmailExpired(string id) => OnCriticalEmailExpired?.Invoke(id);
}