using UnityEngine;

[CreateAssetMenu(menuName = "Tasks/Document Task")]
public class DocumentTaskData : ScriptableObject {

    public string documentTitle;

    [TextArea(4, 10)]
    public string bodyText;
    // usa {0}, {1}, {2} para marcar lacunas
    // ex: "A reunio foi presidida por {0} e durou {1} minutos."

    public BlankSlot[] blanks;

    // departamento correto para este documento > o jogador tem de deduzir com base no contedo
    // no  mostrado diretamente na UI para manter a tenso de "ser que estou a arquivar no stio certo?"
    public ArchiveScript.DepartmentType correctDepartment;

    [System.Serializable]
    public class BlankSlot {
        public string slotID;
        public string correctAnswer;
        public string[] wrongOptions;

        [Header("Peso narrativo")]
        public float weightDenuncia;
        public float weightExtorsao;
        public float weightLealdade;

        // peso que vai para o Company Awareness ao ser arquivado
        [Header("Impacto no Company Awareness")]
        [Range(0f, 1f)] public float awarenessWeight;
    }
}