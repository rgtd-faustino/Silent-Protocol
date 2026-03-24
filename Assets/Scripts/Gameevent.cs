using System;

// bus de eventos central do jogo, em vez de scripts chamarem uns aos outros diretamente, disparam eventos aqui —> quem precisar de reagir subscreve
public static class GameEvent {

    // --- Ciclo do dia ---
    public static event Action<int> OnDayChanged; // dia 1-5
    public static event Action OnWorkHoursStarted; // tasks do dia devem aparecer
    public static event Action OnLunchStarted;
    public static event Action OnAfternoonStarted;
    public static event Action OnNightStarted;
    public static event Action OnDayEnded; // hora de dormir / guardar progresso

    // --- Estado do jogo ---
    public static event Action OnGameOver;
    public static event Action<int> OnEndingReached; // 0=denúncia, 1=extorsão, 2=lealdade

    // --- Intel ---

    // --- Suspeita ---
    public static event Action<SuspicionManager.SuspicionState> OnSuspicionStateChanged;


    public static void DayChanged(int day) => OnDayChanged?.Invoke(day);
    public static void WorkHoursStarted() => OnWorkHoursStarted?.Invoke();
    public static void LunchStarted() => OnLunchStarted?.Invoke();
    public static void AfternoonStarted() => OnAfternoonStarted?.Invoke();
    public static void NightStarted() => OnNightStarted?.Invoke();
    public static void DayEnded() => OnDayEnded?.Invoke();
    public static void GameOver() => OnGameOver?.Invoke();
    public static void EndingReached(int ending) => OnEndingReached?.Invoke(ending);
    public static void SuspicionStateChanged(SuspicionManager.SuspicionState s) => OnSuspicionStateChanged?.Invoke(s);
}