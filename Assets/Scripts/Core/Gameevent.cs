// #my_code - Sistema de eventos desacoplados (publisher/subscriber) que substitui polling de booleans
using System;

public static class GameEvent {

    // passamos o int do dia para os scripts que precisam de saber logo, tipo o TaskManager que vai buscar o schedule e o UIManager para atualizar o ecrã
    public static event Action<int> OnDayChanged;

    // dispara quando as horas avançam no TimeManager. o TaskManager acorda com OnWorkHoursStarted
    public static event Action OnWorkHoursStarted;
    public static event Action OnLunchStarted;
    public static event Action OnAfternoonStarted;
    public static event Action OnNightStarted;

    // quando se dorme ou acaba o tempo, o DayManager apanha isto para ver se avança o dia ou se acaba o jogo
    public static event Action OnDayEnded;

    public static event Action OnGameOver;

    // passa o id do final para o GameManager dar load da cutscene ou UI certo
    public static event Action<int> OnEndingReached;

    public static event Action<int> OnDayStarted;

    // o NPCManager fica à escuta nisto para mudar o behavior dos npcs (tipo patrulhas mais lixadas no estado Investigation)
    public static event Action<SuspicionManager.SuspicionState> OnSuspicionStateChanged;

    // manda o nível de perigo das câmaras para a cena do UI piscar a vermelho
    public static event Action<float> OnCameraOveruseWarning;

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

    // o trigger para quando o MeetingEavesdropScript pode começar a deixar o jogador escutar, senão podiam clicar antes dos NPCs chegarem
    public static event Action OnMeetingStarted;
    public static void MeetingStarted() => OnMeetingStarted?.Invoke();

    // o EmailManager dispara isto com o ID para dar trigger do aviso vermelho na inbox. e tem timeout, por isso há o Expired
    public static event Action<string> OnCriticalEmailAvailable;
    public static event Action<string> OnCriticalEmailExpired;
    public static void CriticalEmailAvailable(string id) => OnCriticalEmailAvailable?.Invoke(id);
    public static void CriticalEmailExpired(string id) => OnCriticalEmailExpired?.Invoke(id);
}
