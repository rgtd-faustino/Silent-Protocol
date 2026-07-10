// #my_code - Sistema de eventos desacoplados (publisher/subscriber) que substitui polling de booleans
using System;

// bus de eventos central do jogo, em vez de scripts chamarem uns aos outros diretamente, disparam eventos aqui —> quem precisar de reagir subscreve
public static class GameEvent
{

    // --- Ciclo do dia ---
    public static event Action<int> OnDayChanged; // dia 1-5
    public static event Action OnWorkHoursStarted; // tasks do dia devem aparecer
    public static event Action OnLunchStarted;
    public static event Action OnAfternoonStarted;
    public static event Action OnNightStarted;
    public static event Action OnDayEnded; // hora de dormir / guardar progresso

    // --- Estado do jogo ---
    public static event Action OnGameOver;
    public static event Action<int> OnEndingReached; // 1=relatório bom, 2=relatório mau, 3=apanhado pela suspeita, 4=exaustão (sono severo)
    public static event Action<int> OnDayStarted;
    public static event Action OnPlayerExhausted; // sono acumulado atingiu o estágio severo (3)


    // --- Suspeita ---
    public static event Action<SuspicionManager.SuspicionState> OnSuspicionStateChanged;

    // --- Câmaras ---
    /// <summary>Disparado quando o uso das câmaras ultrapassa o threshold (para UI e NPCs reagirem).</summary>
    public static event Action<float> OnCameraOveruseWarning; // float = nível de perigo [0-1]


    public static void DayChanged(int day) => OnDayChanged?.Invoke(day);
    public static void WorkHoursStarted() => OnWorkHoursStarted?.Invoke();
    public static void LunchStarted() => OnLunchStarted?.Invoke();
    public static void AfternoonStarted() => OnAfternoonStarted?.Invoke();
    public static void NightStarted() => OnNightStarted?.Invoke();
    public static void DayEnded() => OnDayEnded?.Invoke();
    public static void GameOver() => OnGameOver?.Invoke();
    public static void EndingReached(int ending) => OnEndingReached?.Invoke(ending);
    public static void SuspicionStateChanged(SuspicionManager.SuspicionState s) => OnSuspicionStateChanged?.Invoke(s);
    public static void CameraOveruseWarning(float dangerLevel) => OnCameraOveruseWarning?.Invoke(dangerLevel);
    public static void DayStarted(int day) => OnDayStarted?.Invoke(day);
    public static void PlayerExhausted() => OnPlayerExhausted?.Invoke();

    // --- Reunião ---
    public static event Action OnMeetingStarted;
    public static void MeetingStarted() => OnMeetingStarted?.Invoke();

    // --- Email Crítico ---
    public static event Action<string> OnCriticalEmailAvailable;
    public static event Action<string> OnCriticalEmailExpired;
    public static void CriticalEmailAvailable(string id) => OnCriticalEmailAvailable?.Invoke(id);
    public static void CriticalEmailExpired(string id) => OnCriticalEmailExpired?.Invoke(id);
}