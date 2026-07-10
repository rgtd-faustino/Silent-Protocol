using UnityEngine;

public class StatusEffect
{
    public float duration;
    public float timer;

    // Delegate onde passamos a função do TimeManager. Usamos isto para conseguir intercetar a lógica e injetar debuffs sem precisar de dezenas de if statements manhosos pelo código.
    public System.Func<int, int, float, int> modifySleepStage;

    public StatusEffect(float duration, System.Func<int, int, float, int> modify)
    {
        this.duration = duration;
        this.modifySleepStage = modify;
        timer = 0f;
    }

    public bool UpdateEffect(float deltaMinutes)
    {
        timer += deltaMinutes;
        return timer >= duration;
    }
}
