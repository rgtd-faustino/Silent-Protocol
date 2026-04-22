using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Cutscene")]
public class DialogueCutscene : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 4)] public string text;
}