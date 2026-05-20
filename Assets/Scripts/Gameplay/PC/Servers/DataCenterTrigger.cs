using UnityEngine;

public class DataCenterTrigger : InteractableObject
{
    public enum TriggerType { Entrance, AccessLogs, Server2 }

    [SerializeField] private TriggerType triggerType;
    [SerializeField] private DialogueCutscene cutscene;
    [SerializeField] private IntelItem intelToGive; // s para o Server2

    private bool triggered = false;

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
                    // ao terminar o dilogo d a intel automaticamente
                    if (intelToGive != null)
                        IntelInventory.Instance.AdicionarIntel(intelToGive);
                });
                break;
        }
    }
}