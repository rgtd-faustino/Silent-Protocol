using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightHUDController : MonoBehaviour {
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
    [SerializeField] private Color colorFull = new Color(1.00f, 0.96f, 0.75f); // branco quente
    [SerializeField] private Color colorMid = new Color(0.96f, 0.78f, 0.26f); // âmbar
    [SerializeField] private Color colorLow = new Color(0.83f, 0.38f, 0.10f); // laranja-vermelho
    [SerializeField] private Color colorCritical = new Color(0.55f, 0.10f, 0.04f); // vermelho escuro

    private CanvasGroup canvasGroup;
    private Coroutine flickerRoutine;
    private bool isDead = false;

    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update() {
        float ratio = FlashlightController.Instance.GetBatteryRatio();
        UpdateVisuals(ratio);
    }

    private void UpdateVisuals(float ratio) {
        filamentLit.fillAmount = ratio; // filamento enche de baixo para cima

        Color color = GetColor(ratio);

        // filamento lit
        filamentLit.color = color;

        // bloom —> mais intenso quando cheio
        bulbBloom.color = new Color(color.r, color.g, color.b, ratio > 0 ? Mathf.Lerp(0.08f, 0.55f, ratio) : 0f);

        // fill interior suave
        bulbFill.color = new Color(color.r, color.g, color.b, ratio > 0 ? ratio * 0.15f : 0f);

        // textos
        pctText.text = ratio > 0 ? Mathf.RoundToInt(ratio * 100f) + "%" : "";
        pctText.color = new Color(color.r, color.g, color.b, ratio > 0 ? 0.85f : 0.15f);
        //labelText.color = new Color(color.r, color.g, color.b, 0.35f);

        // animação de ligar e desligar para parecer que fundiu
        if (ratio <= 0f && !isDead) {
            isDead = true;
            StopFlicker();
            StartCoroutine(DeathSequence());

        } else if (ratio > 0f) {
            isDead = false;

            if (ratio <= 0.10f) 
                EnsureFlicker(0.07f, 0.25f); // animação agressiva
            else if (ratio <= 0.25f) 
                EnsureFlicker(0.25f, 0.70f); // animação suave
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
            // 40% de chance de piscar em cada tick para parecer que está a morrer
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
        // animação final rápida
        for (int i = 0; i < 10; i++) {
            canvasGroup.alpha = Random.Range(0.05f, 0.95f);
            yield return new WaitForSeconds(0.05f);
        }

        // faísca branca
        filamentLit.color = Color.white;
        bulbBloom.color = new Color(1f, 1f, 1f, 0.9f);
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.07f);

        // fade para escuro
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