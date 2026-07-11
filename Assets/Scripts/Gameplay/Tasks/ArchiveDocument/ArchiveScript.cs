using System.Collections;
using UnityEngine;

public class ArchiveScript : InteractableObject
{
    // representa os diferentes departamentos no escritório
    // a ideia é o jogador ter de explorar o cenário para perceber qual é qual
    public enum DepartmentType
    {
        RecursosHumanos,
        Financeiro,
        Operacoes
    }

    // configurado no Inspector, diz-nos a que departamento este armário físico corresponde
    [SerializeField] private DepartmentType department;

    protected override void Awake()
    {
        base.Awake();
        objectName = $"Arquivo - {department}";
        tooltipMessage = "Não tens nenhum documento para arquivar";
        // fazemos uma corrotina para não usarmos um update que seria muito ineficiente
        StartCoroutine(TooltipUpdateRoutine());
    }


    private IEnumerator TooltipUpdateRoutine() {
        while (true) {
            UpdateTooltip();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateTooltip() {
        if (PlayerController.Instance.heldDocument == null) {
            tooltipMessage = "E para arquivar documento";
        } else {
            tooltipMessage = "Não tens nenhum documento para arquivar";
        }
    }


    // o objeto só fica com o contorno brilhante se tivermos um documento na mão, servindo como pista visual
    protected override bool CheckShouldGlowByDefault()
    {
        return PlayerController.Instance.heldDocument != null;
    }

    // liga-se ao DocumentManager para registar se a decisão está certa ou errada para verificar se adicionamos suspeita de company awareness ou não (que é verificada no método archiveDocument)
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
        }
    }
}