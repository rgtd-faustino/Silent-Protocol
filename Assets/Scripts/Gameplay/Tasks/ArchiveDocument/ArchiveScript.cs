using UnityEngine;

public class ArchiveScript : InteractableObject {

    // os três arquivos físicos do escritório
    // o jogador vai descobrir qual é qual ao explorar (etiquetas nos armários, conversas com NPCs, etc.)
    public enum DepartmentType {
        RecursosHumanos,
        Financeiro,
        Operacoes
    }

    [SerializeField] private DepartmentType department;



    private void Awake() {
        objectName = $"Arquivo — {department}";
    }


    public override void Interact() {

        // sem documento na mão —> feedback e sai
        if (PlayerController.Instance.heldDocument == null) {
            Debug.Log($"[ArchiveScript] Não tens nenhum documento para arquivar.");
            return;
        }

        DocumentTaskData doc = PlayerController.Instance.heldDocument;
        bool correct = (doc.correctDepartment == department);

        // regista o arquivo no DocumentManager (aplica pesos, atualiza company awareness)
        DocumentManager.Instance.ArchiveDocument(doc, department, correct);

        // completa a task —> correto ou não, a task está feita; a penalidade vem pelo SuspicionManager
        TaskManager.Instance.CompleteTask("Arquivar documento", correct);

        // remove o documento da mão do jogador
        PlayerController.Instance.heldDocument = null;

        if (correct) {
            Debug.Log($"[ArchiveScript] Documento arquivado corretamente em '{department}'.");

        } else {
            Debug.Log($"[ArchiveScript] Departamento errado! Documento de '{doc.correctDepartment}' arquivado em '{department}'.");

            // engano de departamento levanta suspeita imediata —> parece incompetência ou sabotagem
            // IncreaseSuspicion recebe o level (1-3) e a source; usamos 1.5 porque é um erro claro mas não catastrófico
            SuspicionManager.Instance.IncreaseSuspicion(1.5f, GetInstanceID(), SuspicionManager.SuspicionSource.DocumentMisfiled);
        }
    }
}