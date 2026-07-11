using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Cutscene")]
public class DialogueCutscene : ScriptableObject
{
    // contém quem está a falar, contém o texto do diálogo
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 4)] public string text;
}