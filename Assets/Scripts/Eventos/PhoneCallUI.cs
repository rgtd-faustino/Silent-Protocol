using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneCallUI : MonoBehaviour
{

    public static PhoneCallUI Instance;
    public static bool IsOpen => Instance != null && Instance.callPanel != null && Instance.callPanel.activeSelf;


    [Header("Painel raiz")]
    [SerializeField] private GameObject callPanel;

    [Header("Botões de canal (exatamente 3 slots)")]
    [SerializeField] private Button[] channelButtons;
    [SerializeField] private TextMeshProUGUI[] channelLabels;
    [SerializeField] private TextMeshProUGUI[] channelStatusLabels;

    [Header("Transcript")]
    [SerializeField] private TextMeshProUGUI txtTranscript; // texto que vai mostrar a conversa da chamada
    [SerializeField] private ScrollRect transcriptScroll;

    [Header("Prompt")]
    [SerializeField] private TextMeshProUGUI txtCapturePrompt; // aviso para o jogador saber que tem de clicar no botão

    [Header("Footer")]
    [SerializeField] private TextMeshProUGUI txtActiveChannels;

    [Header("Captura")]
    [SerializeField] private GameObject captureIndicator;
    [SerializeField] private Button captureButton;


    private PhoneCallData[] channels;
    private int numChannels   = 0;
    private int activeChannel = 0;

    // estas arrays precisam de bater certo com o número de canais, usamos listas para o texto ficar dinâmico em vez de concatenar strings enormes
    private List<string>[] transcripts; // lista de linhas de diálogo da conversa da chamada
    private int[] missedKeywords;
    private bool[] channelFinished;
    private Coroutine[] channelCoroutines; // uma corrotina para cada chamada

    private bool captureWindowOpen = false;
    private bool capturedThisWindow = false;


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        callPanel.SetActive(false);

        for (int i = 0; i < channelButtons.Length; i++) {
            int idx = i;
            // para cada botão que mostra cada chamada adicionamos a função de selecionar a UI que mostra o seu diálogo
            channelButtons[i].onClick.AddListener(() => SelectChannel(idx));
        }

        captureButton.onClick.AddListener(OnCaptureButtonPressed);
    }

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.X)) { 
            CloseCall(); 
            return; 
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectChannel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectChannel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectChannel(2);
    }

    // quando o jogador clica no telefone mostramos a UI e começa o mini jogo preparando os canais todos e a UI
    public void OpenCall(PhoneCallData[] callData)
    {
        if (IsOpen) return;

        channels = callData; // apanhamos os canais de conversas que estão no telefone em questão
        numChannels = Mathf.Min(channels != null ? channels.Length : 0, 3);
        if (numChannels == 0) return;

        transcripts = new List<string>[3];
        missedKeywords = new int[3];
        channelFinished = new bool[3];
        channelCoroutines = new Coroutine[3];

        // para cada canal que exista atribuímos às variáveis
        for (int i = 0; i < 3; i++)
        {
            transcripts[i] = new List<string>();
            missedKeywords[i] = 0;
            channelFinished[i] = false;

            bool activo = i < numChannels;
            if (channelButtons[i] != null) 
                channelButtons[i].gameObject.SetActive(activo);
            if (!activo) 
                continue;

            if (channelLabels[i] != null) 
                channelLabels[i].text = channels[i].channelLabel;
            if (channelStatusLabels[i] != null) 
                channelStatusLabels[i].text = "ATIVO";

            // começamos a correr as conversas todas nos seus canais respetivos
            channelCoroutines[i] = StartCoroutine(RunChannel(i));
        }

        // começamos por mostrar o prieiro canal, dizemos que a janela de captura da intel ainda não começou
        // e que ainda não foi capturado nada nesta janela
        activeChannel = 0;
        captureWindowOpen = false;
        capturedThisWindow = false;

        // atualiza o texto para mostrar a conversa todo até ao momento
        RefreshTranscript();
        txtCapturePrompt.text = "";
        captureIndicator.SetActive(false);
        AtualizarCanaisAtivos(); // atualiza o texto que mostra quantos canais de chamada ainda estão ativos

        callPanel.SetActive(true);
        PlayerController.Instance.canMoveRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Input.ResetInputAxes(); // para dar reset ao estado dos inputs para evitar comandos que ficaram presos ao abrir a interface
    }

    // o fecho para logo as rotinas todas para o jogador não continuar a receber info no background e a comer recursos
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

    // seleciona um canal de chamada específico
    public void SelectChannel(int index)
    {
        if (index < 0 || index >= numChannels) return;
        activeChannel= index;
        captureWindowOpen = false;
        capturedThisWindow = false;
        txtCapturePrompt.text = "";
        RefreshTranscript(); // atualiza o texto para mostrar a conversa todo até ao momento
    }

    private IEnumerator RunChannel(int ch) {
        PhoneCallData data = channels[ch]; // apanhamos o canal específico de conversa
        float delay = TimeManager.Instance.ToRealSeconds(data.lineDelayGameMinutes);
        int[] keywordLines = data.keywordLineIndices ?? new int[0]; // índice das linhas de conversa em que há keywork e portanto intel

        for (int line = 0; line < data.lines.Length; line++) {
            transcripts[ch].Add(data.lines[line]); // apanhamos cada linha de diálogo

            // atualiza o texto para mostrar a conversa todo até ao momento
            if (ch == activeChannel)
                RefreshTranscript();

            int kwIndex = System.Array.IndexOf(keywordLines, line);
            bool isKeywordLine = (kwIndex >= 0);

            // se esta linha de diálogo contiver uma keyword ativamos a janela de tempo em que o jogador pode capturar a intel
            if (isKeywordLine) {
                if (ch == activeChannel) {
                    captureWindowOpen = true;
                    capturedThisWindow = false;
                    txtCapturePrompt.text = "<color=#FFD700>Capturar!</color>";
                    captureIndicator.SetActive(true); // ligamos o botão

                    float window = delay;
                    while (window > 0f) {
                        if (capturedThisWindow) break;
                        window -= Time.deltaTime;
                        yield return null;
                    }

                    // após a janela de tempo acabar voltamos a desligar o botão
                    captureWindowOpen = false;
                    captureIndicator.SetActive(false);

                    // mostramos o resultado
                    if (capturedThisWindow) {
                        AwardChannelIntel(ch, kwIndex); // adicionamos a intel ao inventário do jogador
                        txtCapturePrompt.text = "<color=#00FF88>Capturado!</color>";

                    } else {
                        missedKeywords[ch]++;
                        UpdateChannelStatus(ch); // dizemos quantas keywords foram perdidas
                        txtCapturePrompt.text = "<color=#FF4444>Perdeste!</color>";
                    }

                    yield return new WaitForSeconds(0.6f);
                    txtCapturePrompt.text = "";
                } else {
                    missedKeywords[ch]++;
                    UpdateChannelStatus(ch);
                    yield return new WaitForSeconds(delay);
                }
            } else {
                yield return new WaitForSeconds(delay);
            }
        }

        channelFinished[ch] = true; // se chegámos até aqui foi porque capturámos a intel
        transcripts[ch].Add("<color=#555555>[FIM DA CHAMADA]</color>");

        if (ch == activeChannel)
            RefreshTranscript();

        UpdateChannelStatus(ch); // mostramos que o canal acabou

        // verificamos se todos os canais acabaram para que a UI do minijogo possa ser fechada
        bool todosTerminados = true;
        for (int i = 0; i < numChannels; i++)
            if (!channelFinished[i]) {
                todosTerminados = false;
                break;
            }

        if (todosTerminados) {
            yield return new WaitForSeconds(2f);
            CloseCall();
        }
    }

    // adicionamos a intel ao inventário do jogador caso tenha capturado corretamente a linha de diálogo
    private void AwardChannelIntel(int ch, int rewardIndex) {
        PhoneCallData data = channels[ch];
        if (data.intelRewards != null && rewardIndex >= 0 && rewardIndex < data.intelRewards.Length
            && data.intelRewards[rewardIndex] != null) {
            IntelInventory.Instance.AdicionarIntel(data.intelRewards[rewardIndex]);
        }
        Debug.Log($"[PhoneCallUI] Canal {data.channelLabel}: keyword {rewardIndex} capturada.");
    }

    private void RefreshTranscript() {
        txtTranscript.text = string.Join("\n", transcripts[activeChannel]);
        Canvas.ForceUpdateCanvases();
        transcriptScroll.normalizedPosition = new Vector2(0f, 0f);
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
        if (txtActiveChannels == null || channelFinished == null) 
            return;

        int ativos = 0;

        for (int i = 0; i < numChannels; i++)
            if (!channelFinished[i]) ativos++;

        txtActiveChannels.text = $"{ativos} canal(is) ativo(s)";
    }

    // o clique prematuro castiga o jogador na barra de suspeita
    // o SuspicionManager lida depois com o estado
    private void OnCaptureButtonPressed() {
        // ao clicar no botão de capturar verificamos se a janela estava aberta (que está sempre quando se clica no botão)
        // e trocamos o valor que verifica se a intel foi capturada, pois ao clicar no botão o jogador quer recolher e então
        // deixamos o código recolher
        if (captureWindowOpen && !capturedThisWindow) {
            capturedThisWindow = true;

            // senão então adicionamos suspeita porque o jogador falhou ao apanhar a intel
        } else if (!captureWindowOpen) {
            SuspicionManager.Instance.AddInstantSuspicion(0.08f);
            txtCapturePrompt.text = "<color=#FF4444>Interceção detectada!</color>";
            StartCoroutine(LimparPromptApos(1.5f));
            CloseCall();
        }
    }

    private IEnumerator LimparPromptApos(float segundos) {
        yield return new WaitForSeconds(segundos);
        txtCapturePrompt.text = "";
    }
}
