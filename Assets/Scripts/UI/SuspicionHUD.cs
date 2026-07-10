using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspicionHUD : MonoBehaviour
{
    [Header("Anel radial")]
    [SerializeField] private Image ringImage; 

    [Header("Olho")]
    [SerializeField] private RectTransform irisRect; 
    [SerializeField] private Image irisImage; 
    [SerializeField] private RectTransform eyeWhiteRect; 

    [Header("Texto")]
    [SerializeField] private TextMeshProUGUI stateLabel; 
    [SerializeField] private TextMeshProUGUI subLabel; 

    [Header("CanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup; 

    [Header("Cores")]
    [SerializeField] private Color colorNone = new Color(0.53f, 0.53f, 0.50f);
    [SerializeField] private Color colorAttention = new Color(0.73f, 0.46f, 0.09f);
    [SerializeField] private Color colorInvestigation = new Color(0.85f, 0.35f, 0.19f);
    [SerializeField] private Color colorExpulsion = new Color(0.89f, 0.29f, 0.29f);

    private Coroutine pulseCoroutine;
    private Coroutine fadeCoroutine;

    private Vector3 baseEyeScale; 
    private Vector3 baseIrisScale; 

    // O currentRatio é calculado com SmoothDamp a partir do valor real no SuspicionManager
    // Isto garante que o anel visual cresce de forma fluida mesmo se a suspeita der saltos grandes
    private float currentRatio; 
    private float velocity; 

    float eyeMultiplier = 2.0f;
    float irisMultiplier = 1.6f;

    void Start()
    {
        baseEyeScale = eyeWhiteRect.localScale;
        baseIrisScale = irisRect.localScale;
        canvasGroup.alpha = 0.25f;

        HandleStateChanged(SuspicionManager.Instance.GetCurrentState());
    }

    // Usamos os eventos globais para evitar que o SuspicionManager dependa do UI
    void OnEnable()
    {
        GameEvent.OnSuspicionStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameEvent.OnSuspicionStateChanged -= HandleStateChanged;
    }

    // Atualizamos o UI com a interpolação exponencial (SmoothStep elevado a 1.3)
    // Fizemos assim para a pupila só começar a dilatar a sério quando o perigo já é alto
    void Update() {
        float targetRatio = SuspicionManager.Instance.GetSuspicionRatio();
        currentRatio = Mathf.SmoothDamp(currentRatio, targetRatio, ref velocity, 0.2f);

        ringImage.fillAmount = currentRatio;

        float eased = Mathf.SmoothStep(0f, 1f, currentRatio);
        eased = Mathf.Pow(eased, 1.3f);

        float eyeScaleFactor = Mathf.Lerp(1f, 2.2f, eased);
        float irisScaleFactor = Mathf.Lerp(1f, 1.5f, eased);

        eyeWhiteRect.localScale = baseEyeScale * eyeScaleFactor;
        irisRect.localScale = baseIrisScale * irisScaleFactor;

        canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, Mathf.Lerp(0.25f, 1f, currentRatio));

        if (currentRatio > 0.6f) {
            float breathe = Mathf.Sin(Time.time * 2f) * 0.03f;
            eyeWhiteRect.localScale += Vector3.one * breathe;
        }
    }

    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    // Reage à mudança de estado decidida pelo SuspicionManager
    // Altera as cores e os textos de acordo com a gravidade da situação
    private void HandleStateChanged(SuspicionManager.SuspicionState newState)
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);

        switch (newState)
        {
            case SuspicionManager.SuspicionState.None:
                ApplyVisuals(colorNone, "NONE", "Comportamento normal", false);
                SetFade(0.25f, 0.8f);
                break;

            case SuspicionManager.SuspicionState.Attention:
                ApplyVisuals(colorAttention, "ATENÇÃO", "NPCs observam-te", false);
                SetFade(1f, 0.4f);
                break;

            case SuspicionManager.SuspicionState.Investigation:
                ApplyVisuals(colorInvestigation, "INVESTIGAÇÃO", "Guardas em alerta", true);
                SetFade(1f, 0.3f);
                break;

            case SuspicionManager.SuspicionState.Expulsion:
                ApplyVisuals(colorExpulsion, "EXPULSÃO", "GAME OVER", true);
                SetFade(1f, 0.2f);
                break;
        }
    }

    private void ApplyVisuals(Color c, string label, string sub, bool pulse)
    {
        ringImage.color = c;
        irisImage.color = c;

        stateLabel.text = label;
        stateLabel.color = c;
        subLabel.text = sub;

        if (pulse)
            pulseCoroutine = StartCoroutine(PulseRoutine(c));
    }

    private void SetFade(float target, float duration)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(target, duration));
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private IEnumerator PulseRoutine(Color baseColor)
    {
        Color dim = new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);

        while (true)
        {
            yield return LerpImageColor(ringImage, baseColor, dim, 0.7f);
            yield return LerpImageColor(ringImage, dim, baseColor, 0.7f);
        }
    }

    private IEnumerator LerpImageColor(Image img, Color from, Color to, float dur)
    {
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            img.color = Color.Lerp(from, to, t / dur);
            yield return null;
        }
    }
}