using System.Collections.Generic;
using UnityEngine;
using static DocumentTaskData;

public class DocumentManager : MonoBehaviour {

    public static DocumentManager Instance;

    [SerializeField] private DocumentTaskData[] allDocuments;

    // dicionário que mapeia slotID -> palavra escolhida pelo jogador
    private Dictionary<string, string> choices = new Dictionary<string, string>();

    // representa o quanto a empresa aprendeu com os documentos arquivados ao longo dos dias
    // ao contrário da suspeita normal, este valor é permanente, quanto mais documentos comprometedores são arquivados, maior o score
    // 0.0 – 0.4 —> empresa não nota nada de especial
    // 0.4 – 0.7 —> sistema interno começa a investigar
    // 0.7 – 1.0 —> alerta ativo: a empresa sabe que alguém está a manipular documentos
    private float companyAwareness = 0f;
    private const float AwarenessThresholdLow = 0.4f;
    private const float AwarenessThresholdHigh = 0.7f;

    // para disparar os eventos só uma vez por threshold
    private bool firedLowAlert = false;
    private bool firedHighAlert = false;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;

    }


    // retorna um documento aleatório —> chamado pelo WriteDocumentUI quando o painel é aberto a aleatoriedade mantém cada sessão imprevisível
    // TODO -> documento mais simples no dia 1, mais comprometedor no dia 5
    public DocumentTaskData GetDocumentForToday() {
        return allDocuments[Random.Range(0, allDocuments.Length)];
    }


    // chamado pelo ArchiveScript quando o jogador arquiva um documento
    // aplica os pesos narrativos das escolhas do jogador e atualiza o company awareness
    // department e correct são passados para que erros de departamento possam amplificar pesos
    public void ArchiveDocument(DocumentTaskData doc, ArchiveScript.DepartmentType department, bool correctDepartment) {

        float awarenessGain = 0f;

        foreach (BlankSlot blank in doc.blanks) {
            // só aplica pesos se o jogador preencheu este slot
            if (!choices.TryGetValue(blank.slotID, out string chosen))
                continue;

            bool correct = (chosen == blank.correctAnswer);

            // errar o departamento amplifica o impacto dos pesos porque o documento chega a quem não devia
            float deptMultiplier = correctDepartment ? 1f : 1.5f;

            // acumula os pesos narrativos
            Debug.Log($"[DocumentManager] Slot '{blank.slotID}' arquivado em '{department}'" +
                      $" | Denúncia +{blank.weightDenuncia * deptMultiplier:F2}" +
                      $" | Extorsão +{blank.weightExtorsao * deptMultiplier:F2}" +
                      $" | Lealdade +{blank.weightLealdade * deptMultiplier:F2}");

            // o awareness sobe com base no peso do slot e no número de dias passados
            // documentos mais tarde no jogo têm maior impacto porque a empresa já tem contexto
            awarenessGain += blank.awarenessWeight * deptMultiplier * (GameManager.Instance.currentDay / (float)GameManager.TotalDays);
        }

        // se arquivado no departamento errado, a própria ação de "ficheiro mal colocado" já gera awareness independentemente do conteúdo
        if (!correctDepartment)
            awarenessGain += 0.05f;

        companyAwareness = Mathf.Clamp01(companyAwareness + awarenessGain);

        CheckAwarenessThresholds();

        Debug.Log($"[DocumentManager] Company Awareness: {companyAwareness:F2}");
    }


    // verifica se o awareness cruzou algum limiar e dispara as consequências correspondentes
    private void CheckAwarenessThresholds() {

        if (!firedLowAlert && companyAwareness >= AwarenessThresholdLow) {
            firedLowAlert = true;
            Debug.Log("[DocumentManager] Company Awareness: sistema interno começou a cruzar dados.");

            // a partir daqui, o SuspicionManager ganha uma nova fonte passiva de suspeita
            // o jogador vai notar que a barra sobe ligeiramente mesmo sem NPCs a vê-lo
            // implementar: SuspicionManager.Instance.ActivatePassiveSource(SuspicionSource.DocumentFlagged, 0.01f);
        }

        if (!firedHighAlert && companyAwareness >= AwarenessThresholdHigh) {
            firedHighAlert = true;
            Debug.Log("[DocumentManager] Company Awareness: alerta ativo — empresa sabe que algo está errado.");

            // a partir daqui, os guardas têm patrulhas extra e a velocidade de subida da suspeita aumenta
            // implementar: NPCManager.Instance.SetAllPatrolling(true) + aumentar baseIncreaseSpeed no SuspicionManager
        }
    }


    // guarda a palavra escolhida para um dado slotID —> chamado pelo WriteDocumentUI no OnSubmit, uma vez por lacuna
    public void SaveChoice(string slotID, string chosenWord) {
        choices[slotID] = chosenWord;
        Debug.Log($"[DocumentManager] Slot '{slotID}' -> '{chosenWord}'");
    }

    // leitura de uma escolha específica —> útil para sistemas futuros que precisem de saber o que o jogador escreveu num slot concreto sem ter acesso a todo o dicionário
    // (ex: um NPC que comente uma decisão específica do jogador num documento anterior)
    public string GetChoice(string slotID) {
        return choices.TryGetValue(slotID, out string val) ? val : null;
    }

    // devolve o dicionário completo —> usado pelo sistema de finais para calcular para que ending o perfil de escolhas do jogador aponta
    // somando os weightDenuncia, weightExtorsao, weightLealdade de cada BlankSlot correspondente aos slotIDs aqui guardados
    public Dictionary<string, string> GetAllChoices() {
        return choices;
    }

    // leitura do company awareness —> usado pelo sistema de finais para calcular o ending
    // um awareness alto fecha certas opções narrativas (a empresa já está de olho)
    public float GetCompanyAwareness() {
        return companyAwareness;
    }
}