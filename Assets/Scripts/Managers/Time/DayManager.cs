using System.Collections;
using UnityEngine;

public class DayManager : MonoBehaviour
{

    public static DayManager Instance;

    public int CurrentDay { get; private set; } = 1;
    public const int TotalDays = 5;

    [HideInInspector] public bool finalObjectiveCompleted = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // usamos uma corrotina para esperar o fim deste frame. assim garantimos que os outros managers já fizeram o Awake/Start e podem apanhar o evento DayStarted sem problemas de concorrência
        StartCoroutine(ShowTitleNextFrame());
    }

    private IEnumerator ShowTitleNextFrame()
    {
        yield return null;
        GameEvent.DayStarted(CurrentDay);

        if (CurrentDay == 1 && TutorialManager.Instance != null) {
            TutorialManager.Instance.StartTutorial();
        }
    }

    // o TimeManager chama isto quando batem as 08:00 (seja por passar o tempo ou por fazer skip a dormir).
    // propagamos logo a mudança do dia via GameEvent para atualizar a UI e os comportamentos diários.
    // se for o último dia validamos a variável finalObjectiveCompleted para saber qual final carregar
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
            int ending = finalObjectiveCompleted ? 1 : 2;
            GameEvent.EndingReached(ending);
        }
    }

    public void SetCurrentDay(int day) { CurrentDay = day; }
}
