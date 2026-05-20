using UnityEngine;

public class ArchiveScript : InteractableObject
{

    // os trs arquivos fsicos do escritrio
    // o jogador vai descobrir qual  qual ao explorar (etiquetas nos armrios, conversas com NPCs, etc.)
    public enum DepartmentType
    {
        RecursosHumanos,
        Financeiro,
        Operacoes
    }

    [SerializeField] private DepartmentType department;



    protected override void Awake()
    {
        base.Awake();
        objectName = $"Arquivo - {department}";
        tooltipMessage = "E para arquivar documento";
    }

    protected override bool CheckShouldGlowByDefault()
    {
        return PlayerController.Instance.heldDocument != null;
    }


    public override void Interact()
    {

        // sem documento na mo > feedback e sai
        if (PlayerController.Instance.heldDocument == null)
        {
            Debug.Log($"[ArchiveScript] No tens nenhum documento para arquivar.");
            return;
        }

        DocumentTaskData doc = PlayerController.Instance.heldDocument;
        bool correct = (doc.correctDepartment == department);

        // regista o arquivo no DocumentManager (aplica pesos, atualiza company awareness)
        DocumentManager.Instance.ArchiveDocument(doc, department, correct);

        // completa a task > correto ou no, a task est feita; a penalidade vem pelo SuspicionManager
        TaskManager.Instance.CompleteTask("Arquivar documento", correct);

        // remove o documento da mo do jogador
        PlayerController.Instance.heldDocument = null;

        if (correct)
        {
            Debug.Log($"[ArchiveScript] Documento arquivado corretamente em '{department}'.");

        }
        else
        {
            Debug.Log($"[ArchiveScript] Departamento errado! Documento de '{doc.correctDepartment}' arquivado em '{department}'.");

            // engano de departamento levanta suspeita imediata > parece incompetncia ou sabotagem
            // IncreaseSuspicion recebe o level (1-3) e a source; usamos 1.5 porque  um erro claro mas no catastrfico
            SuspicionManager.Instance.IncreaseSuspicion(1.5f, GetInstanceID(), SuspicionManager.SuspicionSource.DocumentMisfiled);
        }
    }
}