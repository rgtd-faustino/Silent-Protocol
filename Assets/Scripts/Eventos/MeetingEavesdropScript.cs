using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeetingEavesdropScript : MonoBehaviour
{

    [System.Serializable]
    public class MeetingLine {
        [TextArea(2, 4)]
        public string text;
        [Tooltip("Tem keyword")]
        public bool hasKeyword;
    }

    [SerializeField] private MeetingLine[] meetingLines; // linhas de diálogo da reunião 
    [SerializeField] private IntelItem[] keywordIntelRewards; // quais os objetos de intel que o jogador pode ganhar de acordo com a linha de diálogo

    // adicionamos suspeita passiva enquanto o jogador estiver ao pé da porta enquanto ocorre a reunião
    // apenas 0.06 para não abusar mas para o jogador entender que não pode abusar
    [SerializeField] private float suspicionRateWhileLoitering = 0.06f;
    private const int SOURCE_ID = 9901;


    [Header("Painel do minijogo")]
    [SerializeField] private GameObject eavesdropPanel;
    [SerializeField] private TextMeshProUGUI txtLinha;
    [SerializeField] private TextMeshProUGUI txtPrompt;
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button captureButton;
    private bool capturePressed = false;

    // efeito para escrever os caracteres no texto das linhas de diálogo
    [SerializeField] private float charDelay = 0.04f;
    [SerializeField] private float lingerAfterLine = 1.8f;

    private bool playerInZone = false;
    private bool minigameActive = false;
    private bool meetingActive = false;
    private int capturedCount = 0;
    [SerializeField] private float interactionRadius = 3f;


    void Awake() {
        if (eavesdropPanel != null) eavesdropPanel.SetActive(false);
        if (captureButton != null) {
            captureButton.onClick.AddListener(() => capturePressed = true);
            captureButton.interactable = false;
        }
    }

    // subscrevemos ao evento para sabermos quando a reunião começa para começarmos a usar este código
    void OnEnable()  => GameEvent.OnMeetingStarted += OnMeetingStarted;
    void OnDisable() => GameEvent.OnMeetingStarted -= OnMeetingStarted;
    private void OnMeetingStarted() {
        meetingActive = true;
        capturedCount = 0;
    }

    // fazemos a deteção com Vector3.Distance
    void Update() {
        float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        playerInZone = dist <= interactionRadius;

        if (!meetingActive || minigameActive) return;

        if (playerInZone) {
            SuspicionManager.Instance.IncreaseSuspicion(1, SOURCE_ID, SuspicionManager.SuspicionSource.RestrictedArea);
            UIManager.Instance.ShowTooltip("E - Escutar reunião");

            if (Input.GetKeyDown(KeyCode.E))
                StartMinigame();
        } else {
            SuspicionManager.Instance.StopIncreasingSuspicion(SOURCE_ID);
        }
    }




    private void StartMinigame()
    {
        minigameActive = true;
        SuspicionManager.Instance.StopIncreasingSuspicion(SOURCE_ID);
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        UIManager.Instance.HideTooltip();

        eavesdropPanel.SetActive(true);
        txtScore.text = "0 intel capturada(s)";
        progressBar.value = 0f;
        captureButton.interactable = false;
        capturePressed = false;

        StartCoroutine(RunMinigame());
    }

    // tinha de ser coroutine por causa do texto o delay de letra a letra
    private IEnumerator RunMinigame()
    {
        for (int i = 0; i < meetingLines.Length; i++)
        {
            // vamos mostrando linha a linha do diálogo
            MeetingLine line = meetingLines[i];
            bool hasKeyword = line.hasKeyword;
            bool captured = false;

            progressBar.value = (float)i / meetingLines.Length;
            capturePressed = false;

            // se houver uma keyword é porque há intel e o jogador pode obtê-la
            txtPrompt.text = hasKeyword ? "<color=#FFD700>Clica para capturar!</color>" : "";
            captureButton.interactable = hasKeyword;

            // efeito de escrever letra a letra
            txtLinha.text = "";
            for (int c = 0; c < line.text.Length; c++) {
                txtLinha.text += line.text[c];

                if (hasKeyword && !captured && capturePressed) {
                    captured = true;
                    capturePressed = false;
                    AwardIntel(i);
                }

                yield return new WaitForSeconds(charDelay);
            }

            // isto define quanto tempo é que o texto da linha de diálogo ficará visível após ser totalmente escrito
            float linger = lingerAfterLine;
            while (linger > 0f) {
                if (hasKeyword && !captured && capturePressed) {
                    captured = true;
                    capturePressed = false;
                    AwardIntel(i);
                }
                linger -= Time.deltaTime;
                yield return null;
            }

            // depois de desaparecer, o jogador passou a janela de tempo para obter a intel
            captureButton.interactable = false;

            if (hasKeyword && !captured)
                txtPrompt.text = "<color=#FF4444>Perdeste esta intel!</color>";

            yield return new WaitForSeconds(0.4f);
        }

        progressBar.value = 1f;
        txtPrompt.text = $"Reunião terminada. Capturou {capturedCount} intel(s).";

        yield return new WaitForSeconds(2f);
        EndMinigame();
    }

    // se o jogador premiu o botão então adicionamos-lhe a intel que acabou de ganhar
    private void AwardIntel(int lineIndex)
    {
        capturedCount++;
            txtScore.text = $"{capturedCount} intel capturada(s)";
            txtPrompt.text = "<color=#00FF88>✓ Capturado!</color>";

        if (keywordIntelRewards != null && lineIndex < keywordIntelRewards.Length && keywordIntelRewards[lineIndex] != null)
        {
            IntelInventory.Instance.AdicionarIntel(keywordIntelRewards[lineIndex]);
        }
    }

    // repõe as variáveis de input e UI para não causar bugs noutras interações com objetos depois disto
    private void EndMinigame()
    {
        minigameActive = false;
        meetingActive  = false;

        eavesdropPanel.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);

        Debug.Log($"[MeetingEavesdrop] Minijogo terminado. Intel: {capturedCount}/{meetingLines.Length}");
    }

}
