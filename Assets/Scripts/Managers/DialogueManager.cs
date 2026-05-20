using System.Collections.Generic;
using UnityEngine;

// Gere o fluxo completo de dilogo:
//  1. recebe o NPCDialogueData do NPC com quem o jogador interagiu
//  2. filtra os tpicos vlidos para o estado atual (suspeita, charisma)
//  3. passa a lista filtrada ao DialogueUI
//  4. quando o jogador escolhe um tpico, avalia o outcome e aplica consequncias
public class DialogueManager : MonoBehaviour
{

    public static DialogueManager Instance;

    // emitido quando o dilogo abre  pode ser usado para parar o tempo ou NPCs
    public event System.Action OnDialogueOpen;
    // emitido quando o dilogo fecha
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
            Debug.LogWarning("[DialogueManager] NPCDialogueData  null.");
            return;
        }

        currentData = data;
        isOpen = true;

        // bloqueia o movimento do jogador enquanto o dilogo est aberto
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        Cursor.visible = true;

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

    // fecha o dilogo e devolve o controlo ao jogador
    public void CloseDialogue()
    {
        if (!isOpen) return;
        isOpen = false;
        currentData = null;

        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
        Cursor.visible = false;

        DialogueUI.Instance.HideDialogue();
        OnDialogueClose?.Invoke();
    }

    // chamado pelo DialogueUI quando o jogador clica num tpico
    public void OnTopicSelected(DialogueTopic topic)
    {
        if (topic == null) return;

        TopicOutcome outcome = topic.Evaluate();

        // mostra a resposta do NPC
        DialogueUI.Instance.ShowNPCResponse(outcome.npcResponse, () => {
            // callback aps o jogador fechar a resposta
            ApplyConsequence(outcome);

            // aps confronto, fecha sempre  o jogador no escolhe mais tpicos
            if (topic.topicType == DialogueTopic.TopicType.Confrontation)
                CloseDialogue();
            else
                DialogueUI.Instance.ReturnToTopics(); // volta ao menu de tpicos
        },outcome);
    }

    // filtra os tpicos do NPCDialogueData conforme:
    //   suspeita alta injeta o tpico de confronto (e esconde os normais)
    //   charisma mnimo do tpico
    //   requiresHighSuspicion: s aparece se suspeita >= threshold
    private List<DialogueTopic> FilterTopics(NPCDialogueData data)
    {
        float suspicion = SuspicionManager.Instance.GetSuspicionRatio();
        int charisma = PlayerStats.Instance.GetCarisma();

        Debug.Log($"[Filter] Suspicion: {suspicion}, Charisma: {charisma}, Total topics: {data.topics.Length}");

        List<DialogueTopic> result = new List<DialogueTopic>();

        for (int i = 0; i < data.topics.Length; i++)
        {
            DialogueTopic t = data.topics[i];

            Debug.Log($"[Filter] Topic '{t.buttonLabel}'  requiredCharisma: {t.requiredCharisma}, requiresHighSuspicion: {t.requiresHighSuspicion}");

            if (charisma < t.requiredCharisma)
            {
                Debug.Log($"[Filter] REJEITADO por carisma ({charisma} < {t.requiredCharisma})");
                continue;
            }

            if (t.requiresHighSuspicion && suspicion < t.suspicionThreshold)
            {
                Debug.Log($"[Filter] REJEITADO por suspeita ({suspicion} < {t.suspicionThreshold})");
                continue;
            }

            result.Add(t);
            if (result.Count >= 3) break;
        }

        Debug.Log($"[Filter] Tpicos aprovados: {result.Count}");
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
                Debug.Log("[DialogueManager] GiveIntel  a implementar com IntelInventory.");
                break;
        }
    }
}   