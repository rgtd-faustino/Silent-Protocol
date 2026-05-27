using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneCallUI : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    // Singleton + flag estática                                             //
    // ------------------------------------------------------------------ //

    public static PhoneCallUI Instance;
    public static bool IsOpen => Instance != null
                                 && Instance.callPanel != null
                                 && Instance.callPanel.activeSelf;

    // ------------------------------------------------------------------ //
    // Referências de UI                                                     //
    // ------------------------------------------------------------------ //

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
    [SerializeField] private Button          closeButton;

    // ------------------------------------------------------------------ //
    // Estado interno                                                        //
    // ------------------------------------------------------------------ //

    private PhoneCallData[] channels;
    private int numChannels   = 0;
    private int activeChannel = 0;

    // por canal: texto acumulado, keywords perdidas, coroutine
    private List<string>[] transcripts;
    private int[]          missedKeywords;
    private bool[]         channelFinished;
    private Coroutine[]    channelCoroutines;

    // flag de captura: activa durante a janela de keyword no canal activo
    private bool captureWindowOpen = false;
    private bool capturedThisWindow = false;

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (callPanel != null) callPanel.SetActive(false);

        // liga botões de canal
        for (int i = 0; i < channelButtons.Length; i++)
        {
            int idx = i;
            if (channelButtons[i] != null)
                channelButtons[i].onClick.AddListener(() => SelectChannel(idx));
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCall);
    }

    void Update()
    {
        if (!IsOpen) return;

        // trocar canal com 1/2/3
        if (Input.GetKeyDown(KeyCode.Escape)) { CloseCall(); return; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectChannel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectChannel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectChannel(2);

        // capturar keyword
        if (captureWindowOpen && !capturedThisWindow && Input.GetKeyDown(KeyCode.Space))
            capturedThisWindow = true; // a coroutine do canal lê esta flag
    }

    // ------------------------------------------------------------------ //
    // API pública                                                           //
    // ------------------------------------------------------------------ //

    public void OpenCall(PhoneCallData[] callData)
    {
        if (IsOpen) return;

        channels    = callData;
        numChannels = Mathf.Min(channels != null ? channels.Length : 0, 3);
        if (numChannels == 0) return;

        // inicializa estruturas por canal
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
            if (channelStatusLabels[i] != null) channelStatusLabels[i].text = "● ACTIVO";

            channelCoroutines[i] = StartCoroutine(RunChannel(i));
        }

        activeChannel = 0;
        captureWindowOpen   = false;
        capturedThisWindow  = false;

        RefreshTranscript();
        if (txtCapturePrompt != null) txtCapturePrompt.text = "";

        callPanel.SetActive(true);
        PlayerController.Instance.canMoveRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Input.ResetInputAxes();
    }

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

    // ------------------------------------------------------------------ //
    // Seleção de canal                                                      //
    // ------------------------------------------------------------------ //

    public void SelectChannel(int index)
    {
        if (index < 0 || index >= numChannels) return;
        activeChannel      = index;
        captureWindowOpen  = false;
        capturedThisWindow = false;

        if (txtCapturePrompt != null) txtCapturePrompt.text = "";
        RefreshTranscript();
    }

    // ------------------------------------------------------------------ //
    // Coroutine por canal                                                   //
    // ------------------------------------------------------------------ //

    private IEnumerator RunChannel(int ch)
    {
        PhoneCallData data     = channels[ch];
        float         delay   = TimeManager.Instance.ToRealSeconds(data.lineDelayGameMinutes);
        int[]         kwLines = data.keywordLineIndices ?? new int[0];

        for (int li = 0; li < data.lines.Length; li++)
        {
            // adiciona linha ao transcript deste canal
            transcripts[ch].Add(data.lines[li]);
            if (ch == activeChannel) RefreshTranscript();

            // verifica se esta linha tem keyword
            int kwIndex = System.Array.IndexOf(kwLines, li);
            bool isKeywordLine = (kwIndex >= 0);

            if (isKeywordLine)
            {
                if (ch == activeChannel)
                {
                    // abre janela de captura
                    captureWindowOpen  = true;
                    capturedThisWindow = false;

                    if (txtCapturePrompt != null)
                        txtCapturePrompt.text = "<color=#FFD700>[SPACE] Capturar!</color>";

                    float window = delay;
                    while (window > 0f)
                    {
                        if (capturedThisWindow) break;
                        window -= Time.deltaTime;
                        yield return null;
                    }

                    captureWindowOpen = false;

                    if (capturedThisWindow)
                    {
                        AwardChannelIntel(ch, kwIndex);
                        if (txtCapturePrompt != null)
                            txtCapturePrompt.text = "<color=#00FF88>✓ Capturado!</color>";
                    }
                    else
                    {
                        missedKeywords[ch]++;
                        UpdateChannelStatus(ch);
                        if (txtCapturePrompt != null)
                            txtCapturePrompt.text = "<color=#FF4444>Perdeste!</color>";
                    }

                    // breve pausa de feedback antes de continuar
                    yield return new WaitForSeconds(0.6f);
                    if (txtCapturePrompt != null) txtCapturePrompt.text = "";
                }
                else
                {
                    // jogador estava noutro canal — perde automaticamente
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

        // canal terminou
        channelFinished[ch] = true;
        transcripts[ch].Add("<color=#555555>[FIM DA CHAMADA]</color>");
        if (ch == activeChannel) RefreshTranscript();
        UpdateChannelStatus(ch);

        // fecha o painel se todos os canais terminaram
        bool todosTerminados = true;
        for (int i = 0; i < numChannels; i++)
            if (!channelFinished[i]) { todosTerminados = false; break; }

        if (todosTerminados)
        {
            yield return new WaitForSeconds(2f);
            CloseCall();
        }
    }

    // ------------------------------------------------------------------ //
    // Helpers                                                               //
    // ------------------------------------------------------------------ //

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

        // scroll automático para o fundo
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
            channelStatusLabels[ch].text = "<color=#555555>■ Terminado</color>";
        else if (missedKeywords[ch] > 0)
            channelStatusLabels[ch].text = $"<color=#FF8800>● {missedKeywords[ch]} perdida(s)</color>";
        else
            channelStatusLabels[ch].text = "● ACTIVO";
    }
}
