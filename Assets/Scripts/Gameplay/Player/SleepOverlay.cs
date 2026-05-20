using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/// Gera os efeitos visuais de sono na cmara com base no sleep stage do TimeManager.
/// SETUP NO UNITY:
/// 1. Cria um Canvas (Screen Space  Overlay, Sort Order alto tipo 100) chamado "SleepEffectCanvas"
/// 2. Dentro do Canvas, cria um Image que cobre o ecr todo (Anchor: stretch/stretch, Left/Right/Top/Bottom = 0)
///    Cor: preto (0,0,0,1), mas com Alpha = 0 inicialmente
///    Chama-lhe "SleepOverlay"
/// 3. Arrasta esse Image para o campo "sleepOverlay" neste script
/// 4. Coloca este script num GameObject vazio na cena (ex: "SleepEffectManager")
/// </summary>
public class SleepEffectController : MonoBehaviour
{
    public static SleepEffectController Instance;

    [Header("Overlay")]
    [Tooltip("Image preta que cobre o ecr todo (Alpha 0 inicialmente)")]
    [SerializeField] private Image sleepOverlay;

    // -----------------------------------------------------------------------
    // Stage 1  Piscar de olhos (blink)
    // -----------------------------------------------------------------------
    [Header("Stage 1  Blink")]
    [Tooltip("Intervalo mnimo entre piscares (segundos reais)")]
    [SerializeField] private float blinkIntervalMin = 8f;
    [Tooltip("Intervalo mximo entre piscares (segundos reais)")]
    [SerializeField] private float blinkIntervalMax = 20f;
    [Tooltip("Quanto tempo demora a 'fechar os olhos' (fade to black)")]
    [SerializeField] private float blinkCloseTime = 0.6f;
    [Tooltip("Quanto tempo os olhos ficam fechados no pico")]
    [SerializeField] private float blinkHoldTime = 0.15f;
    [Tooltip("Quanto tempo demora a 'abrir os olhos' (fade in rpido)")]
    [SerializeField] private float blinkOpenTime = 0.12f;

    // -----------------------------------------------------------------------
    // Stage 2  Blackout com avano de tempo
    // -----------------------------------------------------------------------
    [Header("Stage 2  Blackout")]
    [Tooltip("Intervalo mnimo entre blackouts (segundos reais)")]
    [SerializeField] private float blackoutIntervalMin = 30f;
    [Tooltip("Intervalo mximo entre blackouts (segundos reais)")]
    [SerializeField] private float blackoutIntervalMax = 60f;
    [Tooltip("Tempo de jogo mnimo que avana durante um blackout (minutos)")]
    [SerializeField] private float blackoutSkipMinMin = 15f;
    [Tooltip("Tempo de jogo mximo que avana durante um blackout (minutos)")]
    [SerializeField] private float blackoutSkipMinMax = 30f;
    [Tooltip("Durao do fade-in e fade-out do blackout (segundos reais)")]
    [SerializeField] private float blackoutFadeTime = 1.2f;
    [Tooltip("Quanto tempo o ecr fica completamente preto durante o blackout")]
    [SerializeField] private float blackoutHoldTime = 1.5f;

    // -----------------------------------------------------------------------
    // Stage 3  Game over (ecr preto permanente)
    // -----------------------------------------------------------------------
    [Header("Stage 3  Apagar")]
    [Tooltip("Velocidade do fade final para preto (segundos reais)")]
    [SerializeField] private float stageFadeOutTime = 3f;

    // -----------------------------------------------------------------------
    // Estado interno
    // -----------------------------------------------------------------------
    private int lastStage = -1;         // stage anterior para detetar mudana
    private bool effectRunning = false; // evita sobrepor coroutines
    private bool stage3Triggered = false;

    private Coroutine activeLoopCoroutine = null;

    // -----------------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (sleepOverlay == null)
        {
            Debug.LogError("[SleepEffectController] sleepOverlay no est atribudo! Cria um Image preto no Canvas e arrasta aqui.");
            return;
        }

        SetOverlayAlpha(0f);
    }

    void Update()
    {
        if (TimeManager.Instance == null || sleepOverlay == null) return;

        int stage = TimeManager.Instance.GetEffectiveSleepStage();

        // Detetar mudana de stage
        if (stage != lastStage)
        {
            OnStageChanged(lastStage, stage);
            lastStage = stage;
        }
    }

    // -----------------------------------------------------------------------
    // Chamado quando o stage muda
    // -----------------------------------------------------------------------
    private void OnStageChanged(int from, int to)
    {
        // Para o loop anterior (se existir)
        if (activeLoopCoroutine != null)
        {
            StopCoroutine(activeLoopCoroutine);
            activeLoopCoroutine = null;
        }

        switch (to)
        {
            case 0:
                // Sem fadiga  garante ecr limpo
                StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f, 0.3f));
                stage3Triggered = false;
                break;

            case 1:
                // Piscar de olhos peridico
                StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f, 0.3f));
                activeLoopCoroutine = StartCoroutine(BlinkLoop());
                break;

            case 2:
                // Blackouts com avano de tempo
                StartCoroutine(FadeOverlay(GetOverlayAlpha(), 0f, 0.3f));
                activeLoopCoroutine = StartCoroutine(BlackoutLoop());
                break;

            case 3:
                // Apagar  ecr fica preto permanentemente
                if (!stage3Triggered)
                {
                    stage3Triggered = true;
                    StartCoroutine(Stage3Sequence());
                }
                break;
        }
    }

    // -----------------------------------------------------------------------
    // STAGE 1  Loop de piscares
    // -----------------------------------------------------------------------
    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            // Espera intervalo aleatrio antes do prximo piscar
            float waitTime = Random.Range(blinkIntervalMin, blinkIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Verifica se ainda est no stage 1 (pode ter mudado entretanto)
            if (TimeManager.Instance.GetEffectiveSleepStage() != 1) yield break;

            yield return StartCoroutine(DoBlinkEffect());
        }
    }

    private IEnumerator DoBlinkEffect()
    {
        effectRunning = true;

        // Fechar olhos  lento
        yield return StartCoroutine(FadeOverlay(0f, 1f, blinkCloseTime, EaseInQuad));

        // Manter fechado brevemente
        yield return new WaitForSeconds(blinkHoldTime);

        // Abrir olhos  rpido
        yield return StartCoroutine(FadeOverlay(1f, 0f, blinkOpenTime, EaseOutQuad));

        effectRunning = false;
    }

    // -----------------------------------------------------------------------
    // STAGE 2  Loop de blackouts com avano de tempo
    // -----------------------------------------------------------------------
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

    private IEnumerator DoBlackoutEffect()
    {
        effectRunning = true;

        // Fade para preto (mais lento que o piscar)
        yield return StartCoroutine(FadeOverlay(GetOverlayAlpha(), 1f, blackoutFadeTime, EaseInQuad));

        // Ecr preto  avana tempo de jogo
        float skipMinutes = Random.Range(blackoutSkipMinMin, blackoutSkipMinMax);
        AdvanceGameTime(skipMinutes);

        yield return new WaitForSeconds(blackoutHoldTime);

        // Fade de volta
        yield return StartCoroutine(FadeOverlay(1f, 0f, blackoutFadeTime, EaseOutQuad));

        effectRunning = false;

        Debug.Log($"[SleepEffect] Blackout: avanou {skipMinutes:F0} minutos de jogo.");
    }

    // -----------------------------------------------------------------------
    // STAGE 3  Apagar completamente
    // -----------------------------------------------------------------------
    private IEnumerator Stage3Sequence()
    {
        // Para qualquer loop ativo
        if (activeLoopCoroutine != null)
        {
            StopCoroutine(activeLoopCoroutine);
            activeLoopCoroutine = null;
        }

        // Fade lento para preto total
        yield return StartCoroutine(FadeOverlay(GetOverlayAlpha(), 1f, stageFadeOutTime, EaseInQuad));

        // Ecr fica permanentemente preto
        // Aqui podes chamar GameManager.Instance.GameOver() quando existir
        Debug.Log("[SleepEffect] Stage 3: o jogador apagou. Ecr permanentemente preto.");

        // Por enquanto no faz nada mais  o ecr fica preto
    }

    // -----------------------------------------------------------------------
    // Avanar tempo de jogo (blackout)
    // -----------------------------------------------------------------------
    private void AdvanceGameTime(float minutes)
    {
        float currentMinutes = TimeManager.Instance.GetCurrentTimeInHours() * 60f;
        float newMinutes = currentMinutes + minutes;

        // Wrap se ultrapassar as 24h (1440 min)
        newMinutes %= 1440f;

        TimeManager.Instance.SetCurrentMinutes(newMinutes);

        // Acumula fadiga pelo tempo que passou (o jogador "no dormiu")
        // O TimeManager j acumula no Update, mas como avanamos o tempo artificialmente,
        // foramos a acumulao equivalente aqui via Coffee(0)  no, melhor refletido:
        // O accumulatedSleep  privado, por isso o blackout no adiciona fadiga extra explicitamente.
        // Se quiseres que os blackouts piorem a fadiga, expe um mtodo AddSleepDebt(float hours) no TimeManager.
    }

    // -----------------------------------------------------------------------
    // Utilitrios de fade
    // -----------------------------------------------------------------------

    // Fade genrico com easing opcional
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

    // Easing functions
    private float EaseInQuad(float t) => t * t;
    private float EaseOutQuad(float t) => t * (2f - t);

    // -----------------------------------------------------------------------
    // API pblica  para forar efeitos externamente se precisares
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fora um piscar imediato (til para testar ou eventos especiais).
    /// </summary>
    public void TriggerBlink() => StartCoroutine(DoBlinkEffect());

    /// <summary>
    /// Fora um blackout imediato com avano de tempo aleatrio.
    /// </summary>
    public void TriggerBlackout() => StartCoroutine(DoBlackoutEffect());
}