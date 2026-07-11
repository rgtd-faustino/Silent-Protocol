using UnityEngine;

[CreateAssetMenu(menuName = "Tasks/Document Task")]
public class DocumentTaskData : ScriptableObject {

    public string documentTitle; // título do documento que vai aparecer na tooltip

    // usamos índices como {0} e {1} no texto para marcar as lacunas que o jogador terá de preencher ao realizar a tarefa de escrever documento
    // o script WriteDocumentUI vai processar e injetar tags de rich text do TextMeshPro por cima destes marcadores
    [TextArea(4, 10)]
    public string bodyText;

    public BlankSlot[] blanks;

    // ocultamos esta variável da interface para manter o suspense durante a gameplay
    // o ArchiveScript consulta isto quando o jogador arquiva o papel, para validar se acertou no armário ou não, alterando o valor da company awareness
    public ArchiveScript.DepartmentType correctDepartment;

    // só serve para a tarefa de entrega a NPCs, tem de bater certo com a string configurada no NPCScript alvo, isto é visto na tarefa de entregar documento
    public string correctRecipientID;


    // as blanks são as lacunas que o jogador terá de preencher com as opções mostradas. Uma delas é a correta e cada escolha influencia um final possível
    [System.Serializable]
    public class BlankSlot {
        public string slotID;
        public string correctAnswer;
        public string[] wrongOptions;

        // controla o aumento da barra de alerta corporativo
        // se pormos valores altos, significa que o documento compromete seriamente a segurança da empresa
        [Header("Impacto no Company Awareness")]
        [Range(0f, 1f)] public float awarenessWeight;
    }
}