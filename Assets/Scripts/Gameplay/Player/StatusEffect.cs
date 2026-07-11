using UnityEngine;

public class StatusEffect
{
    public float duration;
    public float timer;

    // delegate onde passamos a função do TimeManager
    // usamos isto para conseguir intercetar a lógica e injetar debuffs sem precisar de if statements
    public System.Func<int, int, float, int> modifySleepStage; 

    public StatusEffect(float duration, System.Func<int, int, float, int> modify) // (duration, (realStage, currentStage, timerMinutes)
    {
        this.duration = duration;
        modifySleepStage = modify;
        timer = 0f;
    }

    public bool UpdateEffect(float deltaMinutes)
    {
        timer += deltaMinutes;
        return timer >= duration;
    }
}
