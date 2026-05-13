using System.Collections;
using TMPro;
using UnityEngine;

public class DayTitleUI : MonoBehaviour
{

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float holdDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;



    void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    void OnEnable()
    {
        GameEvent.OnDayStarted += HandleDayStarted;
    }

    void OnDisable()
    {
        GameEvent.OnDayStarted -= HandleDayStarted;
    }

    private void HandleDayStarted(int day)
    {
        StopAllCoroutines();
        StartCoroutine(ShowTitle(day));
    }

    private IEnumerator ShowTitle(int day)
    {
        titleText.text = $"Dia {day}";
        

        // fade in
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

        // espera visível
        yield return new WaitForSeconds(holdDuration);

        // fade out
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}