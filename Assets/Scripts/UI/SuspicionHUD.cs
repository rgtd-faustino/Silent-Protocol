using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspicionHUD : MonoBehaviour
{
    [Header("Anel radial")]
    [SerializeField] private Image ringImage; // círculo que enche conforme a suspeita

    [Header("Olho")]
    [SerializeField] private RectTransform irisRect; // íris (parte colorida)
    [SerializeField] private Image irisImage; // para mudar a cor da íris
    [SerializeField] private RectTransform eyeWhiteRect; // parte branca do olho

    [Header("Texto")]
    [SerializeField] private TextMeshProUGUI stateLabel; // texto principal (estado)
    [SerializeField] private TextMeshProUGUI subLabel; // texto secundário

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
    private Vector3 baseIrisScale; // tamanho inicial da íris

    private float currentRatio; // valor suavizado da suspeita (0-1)
    private float velocity; // usado pelo SmoothDamp

    // até onde o olho e a íris podem crescer
    float eyeMultiplier = 2.0f;
    float irisMultiplier = 1.6f;

    void Start()
    {
        // guardar o tamanho original para depois escalar a partir daqui
        baseEyeScale = eyeWhiteRect.localScale;
        baseIrisScale = irisRect.localScale;
    }

    void OnEnable()
    {
        // subscreve ao evento global de mudança de estado
        GameEvent.OnSuspicionStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameEvent.OnSuspicionStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        // vai buscar o valor da suspeita (0 a 1)
        float targetRatio = SuspicionManager.Instance.GetSuspicionRatio();

        // suaviza o valor para não ser brusco
        currentRatio = Mathf.SmoothDamp(currentRatio, targetRatio, ref velocity, 0.2f);

        // atualiza o anel radial
        ringImage.fillAmount = currentRatio;

        // -------------------------
        // ESCALA DO OLHO (suave e gradual)
        // -------------------------

        // curva para ficar mais natural
        float eased = Mathf.SmoothStep(0f, 1f, currentRatio);
        eased = Mathf.Pow(eased, 1.3f);

        // olho cresce mais
        float eyeScaleFactor = Mathf.Lerp(1f, 2.2f, eased);

        // íris cresce menos (fica mais realista)
        float irisScaleFactor = Mathf.Lerp(1f, 1.5f, eased);

        // aplica escala
        eyeWhiteRect.localScale = baseEyeScale * eyeScaleFactor;
        irisRect.localScale = baseIrisScale * irisScaleFactor;

        // -------------------------
        // "respiração" quando suspeita está alta
        // -------------------------

        if (currentRatio > 0.6f)
        {
            // faz um pequeno movimento tipo pulsar
            float breathe = Mathf.Sin(Time.time * 2f) * 0.03f;
            eyeWhiteRect.localScale += Vector3.one * breathe;
        }
    }

    // (já não está a ser usado, ficou aqui se for preciso mais tarde)
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
                SetFade(0f, 0.8f); // desaparece
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
        // muda cor do anel e da íris
        ringImage.color = c;
        irisImage.color = c;

        // atualiza texto
        stateLabel.text = label;
        stateLabel.color = c;
        subLabel.text = sub;

        // começa o efeito de pulso se necessário
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