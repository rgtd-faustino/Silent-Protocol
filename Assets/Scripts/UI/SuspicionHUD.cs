using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspicionHUD : MonoBehaviour
{
    [Header("Anel radial")]
    [SerializeField] private Image ringImage; // crculo que enche conforme a suspeita

    [Header("Olho")]
    [SerializeField] private RectTransform irisRect; // ris (parte colorida)
    [SerializeField] private Image irisImage; // para mudar a cor da ris
    [SerializeField] private RectTransform eyeWhiteRect; // parte branca do olho

    [Header("Texto")]
    [SerializeField] private TextMeshProUGUI stateLabel; // texto principal (estado)
    [SerializeField] private TextMeshProUGUI subLabel; // texto secundrio

    [Header("CanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup; // usado para fade in/out do HUD

    [Header("Cores")]
    // cores para cada estado de suspeita
    [SerializeField] private Color colorNone = new Color(0.53f, 0.53f, 0.50f);
    [SerializeField] private Color colorAttention = new Color(0.73f, 0.46f, 0.09f);
    [SerializeField] private Color colorInvestigation = new Color(0.85f, 0.35f, 0.19f);
    [SerializeField] private Color colorExpulsion = new Color(0.89f, 0.29f, 0.29f);

    private Coroutine pulseCoroutine;
    private Coroutine fadeCoroutine;

    private Vector3 baseEyeScale; // tamanho inicial do olho
    private Vector3 baseIrisScale; // tamanho inicial da ris

    private float currentRatio; // valor suavizado da suspeita (0-1)
    private float velocity; // usado pelo SmoothDamp

    // at onde o olho e a ris podem crescer
    float eyeMultiplier = 2.0f;
    float irisMultiplier = 1.6f;

    void Start()
    {
        // guardar o tamanho original para depois escalar a partir daqui
        baseEyeScale = eyeWhiteRect.localScale;
        baseIrisScale = irisRect.localScale;
        canvasGroup.alpha = 0.25f;

        HandleStateChanged(SuspicionManager.Instance.GetCurrentState());
    }

    void OnEnable()
    {
        // subscreve ao evento global de mudana de estado
        GameEvent.OnSuspicionStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameEvent.OnSuspicionStateChanged -= HandleStateChanged;
    }

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

        // alpha do HUD acompanha o ratio mesmo no estado None
        // mnimo 0.25, mximo 1.0  o jogador v sempre algo a mexer
        canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, Mathf.Lerp(0.25f, 1f, currentRatio));

        if (currentRatio > 0.6f) {
            float breathe = Mathf.Sin(Time.time * 2f) * 0.03f;
            eyeWhiteRect.localScale += Vector3.one * breathe;
        }
    }

    // (j no est a ser usado, ficou aqui se for preciso mais tarde)
    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    // -------------------------------------------------------

    private void HandleStateChanged(SuspicionManager.SuspicionState newState)
    {
        // para o pulso anterior se existir
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);

        // muda visuals conforme o estado
        switch (newState)
        {
            case SuspicionManager.SuspicionState.None:
                ApplyVisuals(colorNone, "NONE", "Comportamento normal", false);
                SetFade(0.25f, 0.8f);
                break;

            case SuspicionManager.SuspicionState.Attention:
                ApplyVisuals(colorAttention, "ATENO", "NPCs observam-te", false);
                SetFade(1f, 0.4f);
                break;

            case SuspicionManager.SuspicionState.Investigation:
                ApplyVisuals(colorInvestigation, "INVESTIGAO", "Guardas em alerta", true);
                SetFade(1f, 0.3f);
                break;

            case SuspicionManager.SuspicionState.Expulsion:
                ApplyVisuals(colorExpulsion, "EXPULSO", "GAME OVER", true);
                SetFade(1f, 0.2f);
                break;
        }
    }

    private void ApplyVisuals(Color c, string label, string sub, bool pulse)
    {
        // muda cor do anel e da ris
        ringImage.color = c;
        irisImage.color = c;

        // atualiza texto
        stateLabel.text = label;
        stateLabel.color = c;
        subLabel.text = sub;

        // comea o efeito de pulso se necessrio
        if (pulse)
            pulseCoroutine = StartCoroutine(PulseRoutine(c));
    }

    // fade do HUD
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

    // efeito de "piscar" do anel
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