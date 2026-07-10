using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{

    public static DialogueUI Instance;

    [Header("Painel principal")]
    [SerializeField] private GameObject dialoguePanel;
    private TopicOutcome currentOutcome;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcResponseText;

    [Header("Botoes de topico")]
    [SerializeField] private Button[] topicButtons;
    [SerializeField] private TextMeshProUGUI[] topicButtonLabels;

    [Header("Botao de fechar")]
    [SerializeField] private Button closeButton;

    [Header("Painel de topicos")]
    [SerializeField] private GameObject topicsPanel;

    [Header("Botao Intel")]
    [SerializeField] private Button saveIntelButton;

    private Action pendingCallback;

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

    // Inicializamos a interface de diálogo com os dados base que vêm do DialogueManager
    // Passar o greeting diretamente simplifica o fluxo inicial sem precisarmos de construir um tópico vazio e artificial
    public void ShowDialogue(string npcName, string greeting, List<DialogueTopic> topics)
    {
        dialoguePanel.SetActive(true);
        npcNameText.text = npcName;
        
        ShowResponse(greeting, null); 
        SetupTopicButtons(topics);
        topicsPanel.SetActive(true);
    }

    public void HideDialogue()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        dialoguePanel.SetActive(false);
    }

    // Recebe o callback para podermos esperar que o jogador acabe de ler
    // Verificamos logo o TopicOutcome para saber se ligamos o botão de guardar Intel no dossier
    public void ShowNPCResponse(string response, Action onDone, TopicOutcome outcome = null)
    {
        topicsPanel.SetActive(false);
        pendingCallback = onDone;

        currentOutcome = outcome;
        bool hasIntel = outcome != null && outcome.temIntel && outcome.intelAssociado != null && !outcome.intelJaRecolhida;
        saveIntelButton.gameObject.SetActive(hasIntel);

        ShowResponse(response, onDone);
    }

    // Reverte a UI para o estado de seleção
    // Chamado pelo DialogueManager quando a resposta acaba e o painel não se vai fechar de vez
    public void ReturnToTopics()
    {
        topicsPanel.SetActive(true);
        npcResponseText.text = "";
        saveIntelButton.gameObject.SetActive(false);
    }

    // Limpámos todos os listeners antigos e ligámos o botão ao índice correspondente da lista de tópicos no momento do clique
    // Escondemos os botões que não estão a ser usados se existirem menos opções de conversa que os limites do UI
    private void SetupTopicButtons(List<DialogueTopic> topics)
    {
        for (int i = 0; i < topicButtons.Length; i++)
        {
            if (i < topics.Count)
            {
                int index = i; 
                topicButtons[i].gameObject.SetActive(true);
                topicButtonLabels[i].text = topics[i].buttonLabel;
                topicButtons[i].onClick.RemoveAllListeners();
                topicButtons[i].onClick.AddListener(() => DialogueManager.Instance.OnTopicSelected(topics[index]));
            }
            else
            {
                topicButtons[i].gameObject.SetActive(false);
            }
        }
    }

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

        if (onDone == null) yield break;

        npcResponseText.text += " <color=#aaaaaa>[E para continuar]</color>";
    }

    // Acionado pelo input do jogador quando clica no botão ou pressiona E
    // A ideia é saltar a animação se o jogador der input a meio do texto a escrever-se, e avançar efetivamente no diálogo se já estiver completo
    public void OnContinuePressed()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            string fullText = npcResponseText.text;
            return;
        }

        if (pendingCallback != null)
        {
            Action cb = pendingCallback;
            pendingCallback = null;
            cb();
        }
    }
    
    // Liga ao IntelInventory para guardar logo os detalhes vitais recolhidos na conversa sem esperar pelo fecho do diálogo
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