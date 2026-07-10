using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TutorialFeedPrompt : MonoBehaviour {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI ackPrompt;
    [SerializeField] private Image recDot;
    [SerializeField] private RectTransform selfRect;
    private Canvas parentCanvas;

    private Transform target;
    private Vector2 baseAnchoredPos;
    private float charDelay = 0.03f;

    public void Show(string message, Transform worldTarget, Vector2 screenOffset, bool dismissWithE) {
        target = worldTarget;
        baseAnchoredPos = screenOffset;
        gameObject.SetActive(true);
        
        if (dismissWithE) {
            ackPrompt.text = "[E] Confirmar notificação";
        } else {
            ackPrompt.text = "Conclui a ação";
        }

        StartCoroutine(EntryThenType(message));
        StartCoroutine(PulseRecDot());
        StartCoroutine(PulseAck());
    }

    public void SetCanvas(Canvas canvas) {
        parentCanvas = canvas;
    }

    void Update() {
        if (target == null) return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)parentCanvas.transform, screenPoint, parentCanvas.worldCamera, out Vector2 localPoint);
        selfRect.anchoredPosition = localPoint + baseAnchoredPos;
    }

    private IEnumerator EntryThenType(string message) {
        for (int i = 0; i < 3; i++) {
            canvasGroup.alpha = 1;
            selfRect.anchoredPosition += new Vector2(Random.Range(-4f, 4f), Random.Range(-4f, 4f));
            yield return new WaitForSeconds(0.03f);
            canvasGroup.alpha = 0.3f;
            yield return new WaitForSeconds(0.02f);
        }
        canvasGroup.alpha = 1;

        bodyText.text = "";
        foreach (char c in message) {
            bodyText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    private IEnumerator PulseRecDot() {
        while (true) {
            float a = (Mathf.Sin(Time.time * 4f) + 1f) / 2f;
            Color c = recDot.color;
            c.a = Mathf.Lerp(0.3f, 1f, a);
            recDot.color = c;
            yield return null;
        }
    }

    private IEnumerator PulseAck() {
        while (true) {
            float a = (Mathf.Sin(Time.time * 2.5f) + 1f) / 2f;
            Color c = ackPrompt.color;
            c.a = Mathf.Lerp(0.4f, 1f, a);
            ackPrompt.color = c;
            yield return null;
        }
    }

    public void Dismiss() {
        StopAllCoroutines();
        StartCoroutine(ExitAndDestroy());
    }

    private IEnumerator ExitAndDestroy() {
        for (int i = 0; i < 3; i++) {
            selfRect.anchoredPosition += new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            canvasGroup.alpha = Random.Range(0.2f, 1f);
            yield return new WaitForSeconds(0.025f);
        }
        selfRect.localScale = Vector3.one;
        float t = 0;
        while (t < 0.15f) {
            t += Time.deltaTime;
            selfRect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t / 0.15f);
            yield return null;
        }
        Destroy(gameObject);
    }
}