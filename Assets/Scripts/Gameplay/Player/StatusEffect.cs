using UnityEngine;

public class StatusEffect
{
    public float duration;
    public float timer;

    // realStage = sono real
    // currentStage = sono atual com efeitos
    // time = tempo desde que comeou o efeito
    public System.Func<int, int, float, int> modifySleepStage;

    public StatusEffect(float duration, System.Func<int, int, float, int> modify)
    {
        this.duration = duration;
        this.modifySleepStage = modify;
        timer = 0f;
    }

    // Retorna true se o efeito terminou
    public bool UpdateEffect(float deltaMinutes) // recebe minutos de jogo
    {
        timer += deltaMinutes;
        return timer >= duration;
    }
}
