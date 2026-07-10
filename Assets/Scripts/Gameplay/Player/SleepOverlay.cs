using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SleepEffectController : MonoBehaviour
{
    public static SleepEffectController Instance;

    [Header("Overlay")]
    [Tooltip("Imagem do canvas que vamos usar para simular o ecrã a ficar preto.")]
    [SerializeField] private Image sleepOverlay;

    [Header("Stage 1 Blink")]
    [Tooltip("Limite inferior aleatório para os piscares de olhos.")]
    [SerializeField] private float blinkIntervalMin = 8f;
    [Tooltip("Limite superior aleatório para os piscares de olhos.")]
    [SerializeField] private float blinkIntervalMax = 20f;
    [Tooltip("Tempo em segundos que demora a atingir a opacidade máxima e simular um olho a fechar.")]
    [SerializeField] private float blinkCloseTime = 0.6f;
    [Tooltip("Duração do pico do efeito.")]
    [SerializeField] private float blinkHoldTime = 0.15f;
    [Tooltip("Recuperação do piscar, fizemos rápido para dar sobressalto.")]
    [SerializeField] private float blinkOpenTime = 0.12f;

    [Header("Stage 2 Blackout")]
    [Tooltip("Intervalo mais rápido possível para forçar o salto temporal.")]
    [SerializeField] private float blackoutIntervalMin = 30f;
    [Tooltip("Intervalo limite para o salto temporal.")]
    [SerializeField] private float blackoutIntervalMax = 60f;
    [Tooltip("Tamanho mínimo em minutos virtuais do salto do relógio.")]
    [SerializeField] private float blackoutSkipMinMin = 15f;
    [Tooltip("Tamanho máximo do salto no tempo, rouba mesmo muito tempo ao jogador.")]
    [SerializeField] private float blackoutSkipMinMax = 30f;
    [Tooltip("A suavidade da transição de opacidade neste estado.")]
    [SerializeField] private float blackoutFadeTime = 1.2f;
    [Tooltip("Duração da paragem a preto.")]
    [SerializeField] private float blackoutHoldTime = 1.5f;

    [Header("Stage 3 Apagar")]
    [Tooltip("Escurecimento progressivo no fim da run.")]
    [SerializeField] private float stageFadeOutTime = 3f;

    private int lastStage = -1;
    private bool effectRunning = false;
    private bool stage3Triggered = false;

    private Coroutine activeLoopCoroutine = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (sleepOverlay == null)
        {
            Debug.LogError("[SleepEffectController] Esquecemo-nos de associar o Image no editor da Unity.");
            return;
        }

        SetOverlayAlpha(0f);
    }

    void Update()
    {
        if (TimeManager.Instance == null || sleepOverlay == null) return;

        int stage = TimeManager.Instance.GetEffectiveSleepStage();

        if (stage != lastStage)
        {
            OnStageChanged(lastStage, stage);
            lastStage = stage;
        }
    }

    private void OnStageChanged(int from, int to)
    {
        if (activeLoopCoroutine != null)
        {
            StopCoroutine(activeLoopCoroutine);
            activeLoopCoroutine = null;
        }

        switch (to)
        {
            case 0:
                StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f, 0.3f));
                stage3Triggered = false;
                break;

            case 1:
                StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f, 0.3f));
                activeLoopCoroutine = StartCoroutine(BlinkLoop());
                break;

            case 2:
                StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f, 0.3f));
                activeLoopCoroutine = StartCoroutine(BlackoutLoop());
                break;

            case 3:
                if (!stage3Triggered)
                {
                    stage3Triggered = true;
                    StartCoroutine(Stage3Sequence());
                }
                break;
        }
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(blinkIntervalMin, blinkIntervalMax);
            yield return new WaitForSeconds(waitTime);

            if (TimeManager.Instance.GetEffectiveSleepStage() != 1) yield break;

            yield return StartCoroutine(DoBlinkEffect());
        }
    }

    private IEnumerator DoBlinkEffect()
    {
        effectRunning = true;

        yield return StartCoroutine(FadeOverlay(0f, 1f, blinkCloseTime, EaseInQuad));

        yield return new WaitForSeconds(blinkHoldTime);

        yield return StartCoroutine(FadeOverlay(1f, 0f, blinkOpenTime, EaseOutQuad));

        effectRunning = false;
    }

    private IEnumerator BlackoutLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(blackoutIntervalMin, blackoutIntervalMax);
            yield return new WaitForSeconds(waitTime);

            if (TimeManager.Instance.GetEffectiveSleepStage() != 2) yield break;

            yield return StartCoroutine(DoBlackoutEffect());
        }
    }

    // Puxamos diretamente o TimeManager para injetar minutos no relógio. Isto simula lapsos de memória pela privação de sono e prejudica bastante quem abusou das noitadas sem descansar o boneco.
    private IEnumerator DoBlackoutEffect()
    {
        effectRunning = true;

        yield return StartCoroutine(FadeOverlay(GetOverlayAlpha(), 1f, blackoutFadeTime, EaseInQuad));

        float skipMinutes = Random.Range(blackoutSkipMinMin, blackoutSkipMinMax);
        AdvanceGameTime(skipMinutes);

        yield return new WaitForSeconds(blackoutHoldTime);

        yield return StartCoroutine(FadeOverlay(1f, 0f, blackoutFadeTime, EaseOutQuad));

        effectRunning = false;

        Debug.Log($"[SleepEffect] Blackout: avançou {skipMinutes:F0} minutos de jogo.");
    }

    private IEnumerator Stage3Sequence()
    {
        if (activeLoopCoroutine != null)
        {
            StopCoroutine(activeLoopCoroutine);
            activeLoopCoroutine = null;
        }

        yield return StartCoroutine(FadeOverlay(GetOverlayAlpha(), 1f, stageFadeOutTime, EaseInQuad));

        Debug.Log("[SleepEffect] Stage 3: o jogador apagou. Ecrã permanentemente preto.");
    }

    private void AdvanceGameTime(float minutes)
    {
        float currentMinutes = TimeManager.Instance.GetCurrentTimeInHours() * 60f;
        float newMinutes = currentMinutes + minutes;

        newMinutes %= 1440f;

        TimeManager.Instance.SetCurrentMinutes(newMinutes);
    }

    private IEnumerator FadeOverlay(float from, float to, float duration, System.Func<float, float> easing = null)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easing != null ? easing(t) : t;
            SetOverlayAlpha(Mathf.Lerp(from, to, easedT));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (sleepOverlay == null) return;
        Color c = sleepOverlay.color;
        c.a = alpha;
        sleepOverlay.color = c;
    }

    private float GetOverlayAlpha()
    {
        return sleepOverlay != null ? sleepOverlay.color.a : 0f;
    }

    private float EaseInQuad(float t) => t * t;
    private float EaseOutQuad(float t) => t * (2f - t);

    public void TriggerBlink() => StartCoroutine(DoBlinkEffect());

    public void TriggerBlackout() => StartCoroutine(DoBlackoutEffect());
}