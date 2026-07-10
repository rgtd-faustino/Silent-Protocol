using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneCallUI : MonoBehaviour
{

    public static PhoneCallUI Instance;
    public static bool IsOpen => Instance != null  && Instance.callPanel != null && Instance.callPanel.activeSelf;


    [Header("Painel raiz")]
    [SerializeField] private GameObject callPanel;

    [Header("Botões de canal (exactamente 3 slots)")]
    [SerializeField] private Button[]            channelButtons;
    [SerializeField] private TextMeshProUGUI[]   channelLabels;
    [SerializeField] private TextMeshProUGUI[]   channelStatusLabels;

    [Header("Transcript")]
    [SerializeField] private TextMeshProUGUI txtTranscript;
    [SerializeField] private ScrollRect      transcriptScroll;

    [Header("Prompt e fechar")]
    [SerializeField] private TextMeshProUGUI txtCapturePrompt;
    [SerializeField] private Button closeButton;

    [Header("Footer")]
    [SerializeField] private TextMeshProUGUI txtActiveChannels;

    [Header("Captura")]
    [SerializeField] private GameObject captureIndicator;
    [SerializeField] private Button captureButton;


    private PhoneCallData[] channels;
    private int numChannels   = 0;
    private int activeChannel = 0;

    // estas arrays precisam de bater certo com o número de canais, usamos listas para o texto ficar dinâmico em vez de concatenar strings enormes
    private List<string>[] transcripts;
    private int[]          missedKeywords;
    private bool[]         channelFinished;
    private Coroutine[]    channelCoroutines;

    private bool captureWindowOpen = false;
    private bool capturedThisWindow = false;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (callPanel != null) callPanel.SetActive(false);

        for (int i = 0; i < channelButtons.Length; i++)
        {
            int idx = i;
            if (channelButtons[i] != null)
                channelButtons[i].onClick.AddListener(() => SelectChannel(idx));
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCall);

        if (captureButton != null)
            captureButton.onClick.AddListener(OnCaptureButtonPressed);
    }

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.X)) { CloseCall(); return; }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectChannel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectChannel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectChannel(2);
    }


    public void OpenCall(PhoneCallData[] callData)
    {
        if (IsOpen) return;

        channels    = callData;
        numChannels = Mathf.Min(channels != null ? channels.Length : 0, 3);
        if (numChannels == 0) return;

        transcripts      = new List<string>[3];
        missedKeywords   = new int[3];
        channelFinished  = new bool[3];
        channelCoroutines = new Coroutine[3];

        for (int i = 0; i < 3; i++)
        {
            transcripts[i]     = new List<string>();
            missedKeywords[i]  = 0;
            channelFinished[i] = false;

            bool activo = i < numChannels;
            if (channelButtons[i] != null) channelButtons[i].gameObject.SetActive(activo);
            if (!activo) continue;

            if (channelLabels[i] != null)       channelLabels[i].text = channels[i].channelLabel;
            if (channelStatusLabels[i] != null) channelStatusLabels[i].text = "ATIVO";

            channelCoroutines[i] = StartCoroutine(RunChannel(i));
        }

        activeChannel = 0;
        captureWindowOpen   = false;
        capturedThisWindow  = false;

        RefreshTranscript();
        if (txtCapturePrompt != null) txtCapturePrompt.text = "";
        if (captureIndicator != null) captureIndicator.SetActive(false);
        AtualizarCanaisAtivos();

        callPanel.SetActive(true);
        PlayerController.Instance.canMoveRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Input.ResetInputAxes();
    }

    // o fecho pára logo as rotinas todas para o gajo não continuar a receber info no background a comer recursos
    public void CloseCall()
    {
        for (int i = 0; i < 3; i++)
            if (channelCoroutines != null && channelCoroutines[i] != null)
                StopCoroutine(channelCoroutines[i]);

        captureWindowOpen = false;
        callPanel.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Input.ResetInputAxes();
    }


    public void SelectChannel(int index)
    {
        if (index < 0 || index >= numChannels) return;
        activeChannel      = index;
        captureWindowOpen  = false;
        capturedThisWindow = false;

        if (txtCapturePrompt != null) txtCapturePrompt.text = "";
        RefreshTranscript();
    }

    private IEnumerator RunChannel(int ch)
    {
        PhoneCallData data     = channels[ch];
        float         delay   = TimeManager.Instance.ToRealSeconds(data.lineDelayGameMinutes);
        int[]         kwLines = data.keywordLineIndices ?? new int[0];

        for (int li = 0; li < data.lines.Length; li++)
        {
            transcripts[ch].Add(data.lines[li]);
            if (ch == activeChannel) RefreshTranscript();

            int kwIndex = System.Array.IndexOf(kwLines, li);
            bool isKeywordLine = (kwIndex >= 0);

            if (isKeywordLine)
            {
                if (ch == activeChannel)
                {
                    captureWindowOpen  = true;
                    capturedThisWindow = false;

                    if (txtCapturePrompt != null)
                        txtCapturePrompt.text = "<color=#FFD700>Capturar!</color>";
                    if (captureIndicator != null) captureIndicator.SetActive(true);

                    float window = delay;
                    while (window > 0f) {
                        if (capturedThisWindow) break;
                        window -= Time.deltaTime;
                        yield return null;
                    }

                    captureWindowOpen = false;
                    if (captureIndicator != null) captureIndicator.SetActive(false);

                    if (capturedThisWindow) {
                        AwardChannelIntel(ch, kwIndex);
                        if (txtCapturePrompt != null)
                            txtCapturePrompt.text = "<color=#00FF88>Capturado!</color>";
                    } else {
                        missedKeywords[ch]++;
                        UpdateChannelStatus(ch);
                        if (txtCapturePrompt != null)
                            txtCapturePrompt.text = "<color=#FF4444>Perdeste!</color>";
                    }

                    yield return new WaitForSeconds(0.6f);
                    if (txtCapturePrompt != null) txtCapturePrompt.text = "";
                }
                else
                {
                    missedKeywords[ch]++;
                    UpdateChannelStatus(ch);
                    yield return new WaitForSeconds(delay);
                }
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
        }

        channelFinished[ch] = true;
        transcripts[ch].Add("<color=#555555>[FIM DA CHAMADA]</color>");
        if (ch == activeChannel) RefreshTranscript();
        UpdateChannelStatus(ch);

        bool todosTerminados = true;
        for (int i = 0; i < numChannels; i++)
            if (!channelFinished[i]) { todosTerminados = false; break; }

        if (todosTerminados)
        {
            yield return new WaitForSeconds(2f);
            CloseCall();
        }
    }


    private void AwardChannelIntel(int ch, int rewardIndex)
    {
        PhoneCallData data = channels[ch];
        if (data.intelRewards != null
            && rewardIndex >= 0
            && rewardIndex < data.intelRewards.Length
            && data.intelRewards[rewardIndex] != null)
        {
            IntelInventory.Instance.AdicionarIntel(data.intelRewards[rewardIndex]);
        }
        Debug.Log($"[PhoneCallUI] Canal {data.channelLabel}: keyword {rewardIndex} capturada.");
    }

    private void RefreshTranscript()
    {
        if (txtTranscript == null) return;
        txtTranscript.text = string.Join("\n", transcripts[activeChannel]);

        if (transcriptScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            transcriptScroll.normalizedPosition = new Vector2(0f, 0f);
        }
    }

    private void UpdateChannelStatus(int ch)
    {
        if (channelStatusLabels[ch] == null) return;

        if (channelFinished[ch])
            channelStatusLabels[ch].text = "<color=#555555>Terminado</color>";
        else if (missedKeywords[ch] > 0)
            channelStatusLabels[ch].text = $"<color=#FF8800>{missedKeywords[ch]} perdida(s)</color>";
        else
            channelStatusLabels[ch].text = "ATIVO";

        AtualizarCanaisAtivos();
    }

    private void AtualizarCanaisAtivos() {
        if (txtActiveChannels == null || channelFinished == null) return;
        int ativos = 0;
        for (int i = 0; i < numChannels; i++)
            if (!channelFinished[i]) ativos++;
        txtActiveChannels.text = $"{ativos} canal(is) ativo(s)";
    }

    // o clique prematuro castiga bué o jogador na barra de suspeita. o SuspicionManager lida com o estado, nós só chamamos
    private void OnCaptureButtonPressed() {
        if (captureWindowOpen && !capturedThisWindow) {
            capturedThisWindow = true;
        } else if (!captureWindowOpen) {
            SuspicionManager.Instance.AddInstantSuspicion(0.08f);
            if (txtCapturePrompt != null)
                txtCapturePrompt.text = "<color=#FF4444>Interceção detectada!</color>";
            StartCoroutine(LimparPromptApos(1.5f));
            CloseCall();
        }
    }

    private IEnumerator LimparPromptApos(float segundos) {
        yield return new WaitForSeconds(segundos);
        if (txtCapturePrompt != null) txtCapturePrompt.text = "";
    }
}
