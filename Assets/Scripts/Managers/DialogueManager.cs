using System.Collections.Generic;
using UnityEngine;

// Gere o fluxo completo de diálogo:
//  1. recebe o NPCDialogueData do NPC com quem o jogador interagiu
//  2. filtra os tópicos válidos para o estado atual (suspeita, charisma)
//  3. passa a lista filtrada ao DialogueUI
//  4. quando o jogador escolhe um tópico, avalia o outcome e aplica consequências
public class DialogueManager : MonoBehaviour
{

    public static DialogueManager Instance;

    // emitido quando o diálogo abre — pode ser usado para parar o tempo ou NPCs
    public event System.Action OnDialogueOpen;
    // emitido quando o diálogo fecha
    public event System.Action OnDialogueClose;

    private NPCDialogueData currentData;
    private bool isOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // chamado pelo NPCScript.Interact()
    public void OpenDialogue(NPCDialogueData data)
    {
        if (isOpen) return;
        if (data == null)
        {
            Debug.LogWarning("[DialogueManager] NPCDialogueData é null.");
            return;
        }

        currentData = data;
        isOpen = true;

        // bloqueia o movimento do jogador enquanto o diálogo está aberto
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);

        float suspicion = SuspicionManager.Instance.GetSuspicionRatio();

        List<DialogueTopic> filtered = FilterTopics(data);

        // escolhe qual greeting usar
        string greetingToUse = data.greetingText;

        if (suspicion >= data.suspicionThreshold)
        {
            greetingToUse = data.suspicionGreeting;
        }

        DialogueUI.Instance.ShowDialogue(data.displayName, greetingToUse, filtered);

        OnDialogueOpen?.Invoke();
    }

    // fecha o diálogo e devolve o controlo ao jogador
    public void CloseDialogue()
    {
        if (!isOpen) return;
        isOpen = false;
        currentData = null;

        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);

        DialogueUI.Instance.HideDialogue();
        OnDialogueClose?.Invoke();
    }

    // chamado pelo DialogueUI quando o jogador clica num tópico
    public void OnTopicSelected(DialogueTopic topic)
    {
        if (topic == null) return;

        TopicOutcome outcome = topic.Evaluate();

        // mostra a resposta do NPC
        DialogueUI.Instance.ShowNPCResponse(outcome.npcResponse, () => {
            // callback após o jogador fechar a resposta
            ApplyConsequence(outcome);

            // após confronto, fecha sempre — o jogador não escolhe mais tópicos
            if (topic.topicType == DialogueTopic.TopicType.Confrontation)
                CloseDialogue();
            else
                DialogueUI.Instance.ReturnToTopics(); // volta ao menu de tópicos
        },outcome);
    }

    // filtra os tópicos do NPCDialogueData conforme:
    //  — suspeita alta injeta o tópico de confronto (e esconde os normais)
    //  — charisma mínimo do tópico
    //  — requiresHighSuspicion: só aparece se suspeita >= threshold
    private List<DialogueTopic> FilterTopics(NPCDialogueData data)
    {
        Debug.Log("DATA: " + data);
        Debug.Log("TOPICS: " + data.topics);
        Debug.Log("PlayerStats: " + PlayerStats.Instance);
        Debug.Log("SuspicionManager: " + SuspicionManager.Instance);
        float suspicion = SuspicionManager.Instance.GetSuspicionRatio();
        int charisma = PlayerStats.Instance.charisma;

        List<DialogueTopic> result = new List<DialogueTopic>();

        // se suspeita alta e há um tópico de confronto definido, mostra só esse
        

        // caso contrário filtra os tópicos normais
        for (int i = 0; i < data.topics.Length; i++)
        {
            DialogueTopic t = data.topics[i];

            // verificação de charisma mínimo
            if (charisma < t.requiredCharisma)
                continue;

            // tópicos que requerem suspeita alta só aparecem nesse contexto
            if (t.requiresHighSuspicion && suspicion < t.suspicionThreshold)
                continue;

            result.Add(t);

            // limita a 3 tópicos no ecrã (os primeiros válidos)
            if (result.Count >= 3)
                break;
        }

        return result;
    }

    // aplica o efeito do outcome escolhido nos sistemas do jogo
    private void ApplyConsequence(TopicOutcome outcome)
    {
        switch (outcome.consequence)
        {
            case TopicOutcome.ConsequenceType.None:
                break;

            case TopicOutcome.ConsequenceType.DecreaseSuspicion:
                SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(outcome.consequenceAmount, doneCorrectly: true);
                break;

            case TopicOutcome.ConsequenceType.IncreaseSuspicion:
                SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(outcome.consequenceAmount, doneCorrectly: false);
                break;

            case TopicOutcome.ConsequenceType.UnlockFloor:
                if (outcome.unlockFloorIndex >= 0)
                    GameManager.Instance.UnlockFloor(outcome.unlockFloorIndex);
                break;

            case TopicOutcome.ConsequenceType.GiveIntel:
                // IntelInventory.Instance.AddIntel(...) quando o sistema de intel estiver pronto
                Debug.Log("[DialogueManager] GiveIntel — a implementar com IntelInventory.");
                break;
        }
    }
}   