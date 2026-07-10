using UnityEngine;

public class DataCenterTrigger : InteractableObject
{
    public enum TriggerType { Entrance, AccessLogs, Server2 }

    [SerializeField] private TriggerType triggerType;
    [SerializeField] private DialogueCutscene cutscene;
    
    // Objeto de Intel que é adicionado ao inventário assim que a cutscene termina
    [SerializeField] private IntelItem intelToGive;

    // Garante que o jogador não ativa o mesmo diálogo duas vezes ao pisar a zona
    private bool triggered = false;

    // Avalia o tipo de trigger quando o PlayerController entra na área e comunica com a CutsceneDialogueUI para exibir a sequência
    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        switch (triggerType)
        {
            case TriggerType.Entrance:
                CutsceneDialogueUI.Instance.Play(cutscene);
                break;

            case TriggerType.AccessLogs:
                CutsceneDialogueUI.Instance.Play(cutscene);
                break;

            case TriggerType.Server2:
                CutsceneDialogueUI.Instance.Play(cutscene, () =>
                {
                    if (intelToGive != null)
                        IntelInventory.Instance.AdicionarIntel(intelToGive);
                });
                break;
        }
    }
}