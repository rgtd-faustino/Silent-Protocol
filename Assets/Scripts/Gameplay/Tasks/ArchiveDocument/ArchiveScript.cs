using UnityEngine;

public class ArchiveScript : InteractableObject
{
    // Representa os diferentes departamentos no escritório. A ideia é o jogador ter de explorar o cenário e ler etiquetas para perceber qual é qual.
    public enum DepartmentType
    {
        RecursosHumanos,
        Financeiro,
        Operacoes
    }

    // Configurado no Inspector. Diz-nos a que departamento este armário físico corresponde.
    [SerializeField] private DepartmentType department;

    protected override void Awake()
    {
        base.Awake();
        objectName = $"Arquivo - {department}";
        tooltipMessage = "E para arquivar documento";
    }

    // O objeto só fica com o contorno brilhante se tivermos um documento na mão, servindo como pista visual.
    protected override bool CheckShouldGlowByDefault()
    {
        return PlayerController.Instance.heldDocument != null;
    }

    // Liga-se ao DocumentManager para registar se a decisão está certa ou errada e atualizar os pesos da narrativa. A task fica sempre marcada como feita no TaskManager, mas se houver erro chamamos logo o SuspicionManager para aplicar uma penalização.
    public override void Interact()
    {
        if (PlayerController.Instance.heldDocument == null)
        {
            Debug.Log($"[ArchiveScript] Não tens nenhum documento para arquivar.");
            return;
        }

        DocumentTaskData doc = PlayerController.Instance.heldDocument;
        bool correct = (doc.correctDepartment == department);

        DocumentManager.Instance.ArchiveDocument(doc, department, correct);

        TaskManager.Instance.CompleteTask("Arquivar documento", correct);

        PlayerController.Instance.heldDocument = null;

        if (correct)
        {
            Debug.Log($"[ArchiveScript] Documento arquivado corretamente em '{department}'.");
        }
        else
        {
            Debug.Log($"[ArchiveScript] Departamento errado! Documento de '{doc.correctDepartment}' arquivado em '{department}'.");

            // O multiplicador de 1.5 foi escolhido porque consideramos um erro de incompetência claro, mas não grave o suficiente para estragar logo o disfarce. Fica ali no meio termo.
            SuspicionManager.Instance.IncreaseSuspicion(1.5f, GetInstanceID(), SuspicionManager.SuspicionSource.DocumentMisfiled);
        }
    }
}