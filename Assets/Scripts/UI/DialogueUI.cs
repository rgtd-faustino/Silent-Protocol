using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gere o painel de diálogo no ecrã.
// Hierarquia esperada no Canvas:
//   DialoguePanel
//     ├── NPCNameText       (TMP)
//     ├── NPCResponseText   (TMP)
//     ├── TopicsPanel
//     │     ├── TopicButton1
//     │     ├── TopicButton2
//     │     └── TopicButton3
//     └── CloseButton
public class DialogueUI : MonoBehaviour
{

    public static DialogueUI Instance;

    [Header("Painel principal")]
    [SerializeField] private GameObject dialoguePanel;
    private TopicOutcome currentOutcome;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcResponseText;

    [Header("Botões de tópico")]
    // exatamente 3 botões definidos no Inspector
    [SerializeField] private Button[] topicButtons;
    [SerializeField] private TextMeshProUGUI[] topicButtonLabels;

    [Header("Botão de fechar")]
    [SerializeField] private Button closeButton;

    [Header("Painel de tópicos")]
    [SerializeField] private GameObject topicsPanel;

    [Header("Botão Intel")]
    [SerializeField] private Button saveIntelButton;



    // callback após o jogador ver a resposta do NPC (volta aos tópicos ou fecha)
    private Action pendingCallback;

    // velocidade do efeito de typewriter (caracteres por segundo)
    [SerializeField] private float typewriterSpeed = 40f;
    private Coroutine typewriterCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        saveIntelButton.onClick.AddListener(OnSaveIntelPressed);
        saveIntelButton.gameObject.SetActive(false);

        closeButton.onClick.AddListener(OnClosePressed);
        dialoguePanel.SetActive(false);
    }

    // abre o painel e mostra os tópicos disponíveis
    public void ShowDialogue(string npcName, string greeting, List<DialogueTopic> topics)
    {
        dialoguePanel.SetActive(true);
        npcNameText.text = npcName;
        Debug.Log("Abrir diálogo com: " + npcName);

        ShowResponse(greeting, null); // saudação sem callback
        SetupTopicButtons(topics);
        topicsPanel.SetActive(true);
    }

    // esconde o painel por completo
    public void HideDialogue()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        dialoguePanel.SetActive(false);
    }

    // mostra a resposta do NPC com typewriter e esconde os botões de tópico
    // onDone é chamado quando o jogador clicar para continuar após ler a resposta
    public void ShowNPCResponse(string response, Action onDone, TopicOutcome outcome = null)
    {
        topicsPanel.SetActive(false);
        pendingCallback = onDone;

        currentOutcome = outcome;
        bool hasIntel = outcome != null && outcome.temIntel && outcome.intelAssociado != null && !outcome.intelJaRecolhida;
        saveIntelButton.gameObject.SetActive(hasIntel);

        ShowResponse(response, onDone);
    }

    // volta a mostrar os botões de tópico (chamado pelo DialogueManager após uma resposta normal)
    public void ReturnToTopics()
    {
        topicsPanel.SetActive(true);
        npcResponseText.text = "";
        saveIntelButton.gameObject.SetActive(false);
    }

    // configura os 3 botões com os tópicos filtrados
    private void SetupTopicButtons(List<DialogueTopic> topics)
    {
        for (int i = 0; i < topicButtons.Length; i++)
        {
            if (i < topics.Count)
            {
                int index = i; // captura para o closure do lambda
                topicButtons[i].gameObject.SetActive(true);
                topicButtonLabels[i].text = topics[i].buttonLabel;
                topicButtons[i].onClick.RemoveAllListeners();
                topicButtons[i].onClick.AddListener(() => DialogueManager.Instance.OnTopicSelected(topics[index]));
            }
            else
            {
                // esconde botões sem tópico
                topicButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // mostra o texto com efeito typewriter
    // se onDone != null, clicar no painel avança / conclui a leitura
    private void ShowResponse(string text, Action onDone)
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(text, onDone));
    }

    private IEnumerator TypewriterRoutine(string text, Action onDone)
    {
        npcResponseText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            npcResponseText.text += text[i];
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }

        typewriterCoroutine = null;

        // se não há callback (saudação inicial) não precisa de esperar input
        if (onDone == null) yield break;

        // indica ao jogador que pode continuar — pode substituir por um ícone de "continuar"
        npcResponseText.text += " <color=#aaaaaa>[E para continuar]</color>";
    }

    // chamado pelo Update do CameraScript quando carrega E com diálogo aberto
    // o CameraScript já trata do E — vamos usar o botão de fechar no painel por agora
    // mas este método pode ser chamado externamente se necessário
    public void OnContinuePressed()
    {
        if (typewriterCoroutine != null)
        {
            // se o typewriter ainda está a correr, salta para o fim
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            string fullText = npcResponseText.text;
            // remove o indicador se existia parcialmente e re-aplica o texto completo
            // para simplificar, guardamos o texto completo numa var separada
            // ver DialogueUI.ShowResponseFull() — refactor futuro se necessário
            return;
        }

        if (pendingCallback != null)
        {
            Action cb = pendingCallback;
            pendingCallback = null;
            cb();
        }
    }
    private void OnSaveIntelPressed()
    {
        if (currentOutcome == null) return;

        IntelInventory.Instance.AdicionarIntel(currentOutcome.intelAssociado);
        currentOutcome.intelJaRecolhida = true;
        saveIntelButton.gameObject.SetActive(false);
    }
    private void OnClosePressed()
    {
        DialogueManager.Instance.CloseDialogue();
    }
}