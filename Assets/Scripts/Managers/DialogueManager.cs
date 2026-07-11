using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{

    public static DialogueManager Instance;

    // disparamos isto quando a janela de diálogo abre. útil para quem precisar de pausar o tempo ou congelar as animações dos NPCs sem precisarmos de acoplar aqui
    public event System.Action OnDialogueOpen;
    public event System.Action OnDialogueClose;

    private NPCDialogueData currentData;
    private bool isOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // o NPCScript chama isto no Interact(). nós filtramos as falas possíveis com base nas stats do jogador e mandamos a lista limpa para a UI.
    // assim mantemos a lógica das regras separada da interface
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

        // trancamos o movimento do boneco e libertamos o rato, senão a câmara rodava feita maluca quando tentássemos clicar nas respostas
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        Cursor.visible = true;

        float suspicion = SuspicionManager.Instance.GetSuspicionRatio();

        List<DialogueTopic> filtered = FilterTopics(data);

        // se a suspeita estiver alta, o NPC arranca logo a espumar da boca com o greeting de confronto em vez do normal
        string greetingToUse = data.greetingText;

        if (suspicion >= data.suspicionThreshold)
        {
            greetingToUse = data.suspicionGreeting;
        }

        DialogueUI.Instance.ShowDialogue(data.displayName, greetingToUse, filtered);

        OnDialogueOpen?.Invoke();
    }

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

    // este método é o callback da UI. quando o jogador clica numa opção, avaliamos o resultado e aplicamos os buffs ou debuffs.
    // usamos uma callback na UI para aplicar a consequência só depois de mostrar a resposta do NPC, senão a barra de suspeita subia antes do jogador ler o insulto
    public void OnTopicSelected(DialogueTopic topic)
    {
        if (topic == null) return;

        TopicOutcome outcome = topic.Evaluate();

        DialogueUI.Instance.ShowNPCResponse(outcome.npcResponse, () => {
            ApplyConsequence(outcome);

            // os tópicos de confronto têm de fechar sempre a conversa no fim, porque o NPC passa-se da cabeça e recusa-se a falar mais
            if (topic.topicType == DialogueTopic.TopicType.Confrontation)
                CloseDialogue();
            else
                DialogueUI.Instance.ReturnToTopics();
        },outcome);
    }

    // cortamos as opções se o jogador não tiver carisma suficiente ou se o tópico exigir uma situação de alta suspeita para aparecer.
    // limitámos a 3 tópicos de cada vez por causa do layout que fizemos na UI
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

        Debug.Log($"[Filter] Tópicos aprovados: {result.Count}");
        return result;
    }

    // preferimos canalizar a mudança de suspeita pelo método das tasks no SuspicionManager, 
    // assim aproveitamos os mesmos sons de sucesso/falha e mantemos a consistência audível
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
                Debug.Log("[DialogueManager] GiveIntel - a implementar com IntelInventory.");
                break;
        }
    }
}