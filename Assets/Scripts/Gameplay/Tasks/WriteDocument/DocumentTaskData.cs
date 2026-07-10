using UnityEngine;

[CreateAssetMenu(menuName = "Tasks/Document Task")]
public class DocumentTaskData : ScriptableObject {

    public string documentTitle;

    // Usamos índices como {0} e {1} no texto para marcar as lacunas. O script WriteDocumentUI vai processar e injetar tags de rich text do TextMeshPro por cima destes marcadores.
    [TextArea(4, 10)]
    public string bodyText;

    public BlankSlot[] blanks;

    // Ocultamos esta variável da interface para manter o suspense no gameplay. O ArchiveScript consulta isto quando o jogador larga o papel, para validar se acertou no armário.
    public ArchiveScript.DepartmentType correctDepartment;

    // Só serve para a tarefa de entrega a NPCs. Tem de bater certo com a string configurada no NPCScript alvo.
    public string correctRecipientID;

    [System.Serializable]
    public class BlankSlot {
        public string slotID;
        public string correctAnswer;
        public string[] wrongOptions;

        // O DocumentManager vai somando estes valores em background. As escolhas aqui feitas determinam qual dos três finais o jogador desbloqueia, mas não interferem na conclusão imediata da tarefa.
        [Header("Peso narrativo")]
        public float weightDenuncia;
        public float weightExtorsao;
        public float weightLealdade;

        // Controla o aumento da barra de alerta corporativo. Se pormos valores altos, significa que o documento compromete seriamente a segurança da empresa.
        [Header("Impacto no Company Awareness")]
        [Range(0f, 1f)] public float awarenessWeight;
    }
}