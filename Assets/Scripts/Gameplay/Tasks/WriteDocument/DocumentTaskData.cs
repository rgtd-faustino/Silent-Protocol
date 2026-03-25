// DocumentTaskData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Tasks/Document Task")]
public class DocumentTaskData : ScriptableObject {

    public string documentTitle;

    [TextArea(4, 10)]
    public string bodyText;
    // usa {0}, {1}, {2} para marcar lacunas
    // ex: "A reunião foi presidida por {0} e durou {1} minutos."

    public BlankSlot[] blanks;

    [System.Serializable]
    public class BlankSlot {
        public string slotID;        // "slot_dia3_transfer" — para guardar narrativa depois
        public string correctAnswer;
        public string[] wrongOptions;

        [Header("Peso narrativo (para mais tarde)")]
        public float weightDenuncia;
        public float weightExtorsao;
        public float weightLealdade;
    }
}