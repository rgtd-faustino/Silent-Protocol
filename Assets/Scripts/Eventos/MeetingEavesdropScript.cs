using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeetingEavesdropScript : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    // Dados da reunião                                                      //
    // ------------------------------------------------------------------ //

    [Header("Linhas da reunião (por ordem)")]
    [Tooltip("Cada MeetingLine é uma frase. Activa hasKeyword nas que têm intel.")]
    [SerializeField] private MeetingLine[] meetingLines;

    [Header("Intel por cada linha com keyword (mesmo índice)")]
    [SerializeField] private IntelItem[] keywordIntelRewards;

    // ------------------------------------------------------------------ //
    // Suspicion                                                             //
    // ------------------------------------------------------------------ //

    [Header("Suspeita passiva enquanto na zona")]
    [SerializeField] private float suspicionRateWhileLoitering = 0.06f;
    private const int SOURCE_ID = 9901; // ID único para esta fonte de suspicion

    // ------------------------------------------------------------------ //
    // UI                                                                    //
    // ------------------------------------------------------------------ //

    [Header("Painel do minijogo")]
    [SerializeField] private GameObject eavesdropPanel;
    [SerializeField] private TextMeshProUGUI txtLinha;
    [SerializeField] private TextMeshProUGUI txtPrompt;
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private Image progressBar;

    [Header("Timing")]
    [SerializeField] private float charDelay       = 0.04f;  // segundos por carácter
    [SerializeField] private float lingerAfterLine = 1.8f;   // pausa entre linhas (segundos reais)

    // ------------------------------------------------------------------ //
    // Estado interno                                                        //
    // ------------------------------------------------------------------ //

    private bool playerInZone   = false;
    private bool minigameActive = false;
    private bool meetingActive  = false;
    private int  capturedCount  = 0;

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Awake()
    {
        if (eavesdropPanel != null) eavesdropPanel.SetActive(false);
    }

    void OnEnable()  => GameEvent.OnMeetingStarted += OnMeetingStarted;
    void OnDisable() => GameEvent.OnMeetingStarted -= OnMeetingStarted;

    void Update()
    {
        if (!meetingActive || minigameActive) return;

        if (playerInZone)
        {
            // suspicion passiva por loitering
            SuspicionManager.Instance.IncreaseSuspicion(1, SOURCE_ID, SuspicionManager.SuspicionSource.RestrictedArea);
            UIManager.Instance.ShowTooltip("E  — Escutar reunião");

            if (Input.GetKeyDown(KeyCode.E))
                StartMinigame();
        }
        else
        {
            SuspicionManager.Instance.StopIncreasingSuspicion(SOURCE_ID);
        }
    }

    // ------------------------------------------------------------------ //
    // Trigger de zona                                                       //
    // ------------------------------------------------------------------ //

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInZone = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        UIManager.Instance.HideTooltip();
        SuspicionManager.Instance.StopIncreasingSuspicion(SOURCE_ID);
    }

    // ------------------------------------------------------------------ //
    // Evento de reunião                                                     //
    // ------------------------------------------------------------------ //

    private void OnMeetingStarted()
    {
        meetingActive  = true;
        capturedCount  = 0;
    }

    // ------------------------------------------------------------------ //
    // Minijogo                                                              //
    // ------------------------------------------------------------------ //

    private void StartMinigame()
    {
        minigameActive = true;
        SuspicionManager.Instance.StopIncreasingSuspicion(SOURCE_ID);
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        UIManager.Instance.HideTooltip();

        if (eavesdropPanel != null) eavesdropPanel.SetActive(true);
        if (txtScore != null)       txtScore.text = "0 intel capturada(s)";
        if (progressBar != null)    progressBar.fillAmount = 0f;

        StartCoroutine(RunMinigame());
    }

    private IEnumerator RunMinigame()
    {
        for (int i = 0; i < meetingLines.Length; i++)
        {
            MeetingLine line     = meetingLines[i];
            bool        hasKw   = line.hasKeyword;
            bool        captured = false;

            // barra de progresso
            if (progressBar != null)
                progressBar.fillAmount = (float)i / meetingLines.Length;

            // prompt
            if (txtPrompt != null)
                txtPrompt.text = hasKw ? "<color=#FFD700>[SPACE] Capturar!</color>" : "";

            // typewriter — o jogador pode carregar SPACE durante a digitação
            if (txtLinha != null) txtLinha.text = "";
            for (int c = 0; c < line.text.Length; c++)
            {
                if (txtLinha != null) txtLinha.text += line.text[c];

                if (hasKw && !captured && Input.GetKeyDown(KeyCode.Space))
                {
                    captured = true;
                    AwardIntel(i);
                }

                yield return new WaitForSeconds(charDelay);
            }

            // janela extra após a linha terminar
            float linger = lingerAfterLine;
            while (linger > 0f)
            {
                if (hasKw && !captured && Input.GetKeyDown(KeyCode.Space))
                {
                    captured = true;
                    AwardIntel(i);
                }
                linger -= Time.deltaTime;
                yield return null;
            }

            // feedback de falha
            if (hasKw && !captured && txtPrompt != null)
                txtPrompt.text = "<color=#FF4444>Perdeste esta intel!</color>";

            yield return new WaitForSeconds(0.4f);
        }

        // fim
        if (progressBar != null) progressBar.fillAmount = 1f;
        if (txtPrompt != null)   txtPrompt.text = $"Reunião terminada. Capturou {capturedCount} intel(s).";

        yield return new WaitForSeconds(2f);
        EndMinigame();
    }

    private void AwardIntel(int lineIndex)
    {
        capturedCount++;
        if (txtScore != null)
            txtScore.text = $"{capturedCount} intel capturada(s)";
        if (txtPrompt != null)
            txtPrompt.text = "<color=#00FF88>✓ Capturado!</color>";

        if (keywordIntelRewards != null
            && lineIndex < keywordIntelRewards.Length
            && keywordIntelRewards[lineIndex] != null)
        {
            IntelInventory.Instance.AdicionarIntel(keywordIntelRewards[lineIndex]);
        }
    }

    private void EndMinigame()
    {
        minigameActive = false;
        meetingActive  = false;

        if (eavesdropPanel != null) eavesdropPanel.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);

        Debug.Log($"[MeetingEavesdrop] Minijogo terminado. Intel: {capturedCount}/{meetingLines.Length}");
    }

    // ------------------------------------------------------------------ //
    // Estrutura de dados                                                    //
    // ------------------------------------------------------------------ //

    [System.Serializable]
    public class MeetingLine
    {
        [TextArea(2, 4)]
        public string text;
        [Tooltip("Marcar se esta linha contém uma palavra-chave capturável.")]
        public bool hasKeyword;
    }
}
