using System.Collections;
using UnityEngine;

public class DayManager : MonoBehaviour
{

    public static DayManager Instance;

    public int CurrentDay { get; private set; } = 1;
    public const int TotalDays = 5;

    // bool do dia 5 - outros scripts metem isto a true quando o objetivo for cumprido
    [HideInInspector] public bool finalObjectiveCompleted = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // dispara o titulo do primeiro dia logo no arranque
        StartCoroutine(ShowTitleNextFrame());
    }
    private IEnumerator ShowTitleNextFrame()
    {
        yield return null;
        GameEvent.DayStarted(CurrentDay);
    }
    // chamado pelo TimeManager.Sleep() no fim
    public void OnDayEnded()
    {
        Debug.Log($"OnDayEnded chamado! CurrentDay: {CurrentDay}");
        if (CurrentDay < TotalDays)
        {
            CurrentDay++;
            GameEvent.DayChanged(CurrentDay);
            GameEvent.DayStarted(CurrentDay);
        }
        else
        {
            // ultimo dia acabou - decide ending
            int ending = finalObjectiveCompleted ? 1 : 2;
            GameEvent.EndingReached(ending);
        }
    }

    // setter para o SaveManager poder restaurar o dia
    public void SetCurrentDay(int day) { CurrentDay = day; }
}
