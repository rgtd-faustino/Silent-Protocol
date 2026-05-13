using System.Collections;
using TMPro;
using UnityEngine;

public class EndingUI : MonoBehaviour
{

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private float fadeInDuration = 1.2f;

    // textos para cada ending
    private static readonly string[] EndingTitles = {
        "",              // índice 0 não usado
        "Game Over",     // ending 1
        "Game Over"      // ending 2
    };

    private static readonly string[] EndingDescriptions = {
        "",
        "Fizeste a escolha certa.",   // ending 1
        "Devias ter agido."           // ending 2
    };

    void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    void OnEnable()
    {
        GameEvent.OnEndingReached += HandleEnding;
    }

    void OnDisable()
    {
        GameEvent.OnEndingReached -= HandleEnding;
    }

    private void HandleEnding(int ending)
    {
        StartCoroutine(ShowEnding(ending));
    }

    private IEnumerator ShowEnding(int ending)
    {
        // bloqueia o jogador
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(UnityEngine.CursorLockMode.None);

        titleText.text = EndingTitles[ending];
        descriptionText.text = EndingDescriptions[ending];

        // fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}