using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightHUDController : MonoBehaviour {
    public static FlashlightHUDController Instance;

    [Header("Filament")]
    [SerializeField] private Image filamentLit;
    [SerializeField] private Image filamentDead;

    [Header("Glow")]
    [SerializeField] private Image bulbFill;
    [SerializeField] private Image bulbBloom;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI pctText;
    [SerializeField] private TextMeshProUGUI labelText;

    [Header("Colors")]
    [SerializeField] private Color colorFull = new Color(1.00f, 0.96f, 0.75f);
    [SerializeField] private Color colorMid = new Color(0.96f, 0.78f, 0.26f);
    [SerializeField] private Color colorLow = new Color(0.83f, 0.38f, 0.10f);
    [SerializeField] private Color colorCritical = new Color(0.55f, 0.10f, 0.04f);

    private CanvasGroup canvasGroup;
    private Coroutine flickerRoutine;
    private bool isDead = false;

    void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        // deixamos o objeto ativo no arranque mas metemos transparente (porque o jogador não tem a lanterna ainda)
        // e assim os outros scripts podem encontrar as referências sem termos problemas de null
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // se o jogador não tem lanterna ou não é noite, o HUD não fica visível
    void Update() {
        if (!PlayerController.Instance.hasFlashlight || !TimeManager.Instance.isNight) {
            // se o HUD já estava visivel anteriormente então passa a ser desligado (provavelmente porque ficou de dia)
            if (canvasGroup.alpha > 0f) {
                StopAllCoroutines();
                canvasGroup.alpha = 0f;
                isDead = false;
                flickerRoutine = null;
            }
            return;
        }

        // se chegámos até aquié porque o jogador tem lanterna e é de noite, mas se o HUD ainda estivesse escondido e houvesse bateria então fica visível
        if (canvasGroup.alpha == 0f && FlashlightController.Instance.GetBatteryRatio() > 0f) {
            canvasGroup.alpha = 1f;
        }

        float ratio = FlashlightController.Instance.GetBatteryRatio();
        UpdateVisuals(ratio);
    }

    public void Show() {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // tratamos aqui das cores dinâmicas e da lógica de fade, lemos o rácio que apanhamos diretamente do FlashlightController para o filamento reagir visualmente
    private void UpdateVisuals(float ratio) {
        filamentLit.fillAmount = ratio;
        Color color = GetColor(ratio);
        filamentLit.color = color;
        bulbBloom.color = new Color(color.r, color.g, color.b, ratio > 0 ? Mathf.Lerp(0.08f, 0.55f, ratio) : 0f);
        bulbFill.color = new Color(color.r, color.g, color.b, ratio > 0 ? ratio * 0.15f : 0f);
        pctText.text = ratio > 0 ? Mathf.RoundToInt(ratio * 100f) + "%" : "";
        pctText.color = new Color(color.r, color.g, color.b, ratio > 0 ? 0.85f : 0.15f);

        if (ratio <= 0f && !isDead) {
            isDead = true;
            StopFlicker();
            StartCoroutine(DeathSequence());

        } else if (ratio > 0f) {
            isDead = false;

            if (ratio <= 0.10f) 
                EnsureFlicker(0.07f, 0.25f);
            else if (ratio <= 0.25f) 
                EnsureFlicker(0.25f, 0.70f);
            else 
                StopFlicker();
        }
    }

    private Color GetColor(float r) {
        if (r > 0.60f) 
            return Color.Lerp(colorMid, colorFull, (r - 0.60f) / 0.40f);
        if (r > 0.25f) 
            return Color.Lerp(colorLow, colorMid, (r - 0.25f) / 0.35f);
        if (r > 0.10f) 
            return Color.Lerp(colorCritical, colorLow, (r - 0.10f) / 0.15f);

        return colorCritical;
    }

    private void EnsureFlicker(float interval, float minAlpha) {
        if (flickerRoutine == null)
            flickerRoutine = StartCoroutine(FlickerRoutine(interval, minAlpha));
    }

    private IEnumerator FlickerRoutine(float interval, float minAlpha) {
        while (true) {
            if (Random.value < 0.4f)
                canvasGroup.alpha = Random.Range(minAlpha, 1f);
            else
                canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(interval + Random.Range(-0.02f, 0.02f));
        }
    }

    private void StopFlicker() {
        if (flickerRoutine != null) {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator DeathSequence() {
        for (int i = 0; i < 10; i++) {
            canvasGroup.alpha = Random.Range(0.05f, 0.95f);
            yield return new WaitForSeconds(0.05f);
        }

        filamentLit.color = Color.white;
        bulbBloom.color = new Color(1f, 1f, 1f, 0.9f);
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.07f);

        float t = 0f;
        while (t < 0.5f) {
            t += Time.deltaTime;
            float a = 1f - (t / 0.5f);
            filamentLit.color = new Color(0.8f, 0.3f, 0.1f, a);
            bulbBloom.color = new Color(0.8f, 0.3f, 0.1f, a * 0.3f);
            bulbFill.color = Color.clear;
            yield return null;
        }

        filamentLit.color = Color.clear;
        bulbBloom.color = Color.clear;
    }
}