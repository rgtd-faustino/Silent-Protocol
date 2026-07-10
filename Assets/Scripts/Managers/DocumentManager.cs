using System.Collections.Generic;
using UnityEngine;
using static DocumentTaskData;

public class DocumentManager : MonoBehaviour {

    public static DocumentManager Instance;

    [SerializeField] private DocumentTaskData[] allDocuments;

    // guarda as opções que o jogador meteu nas lacunas dos documentos (slotID para a palavra).
    // acumulamos isto durante a semana e no final o jogo usa este dicionário para calcular o perfil do jogador e o ending
    private Dictionary<string, string> choices = new Dictionary<string, string>();

    // isto representa o nível de alerta passivo da empresa perante as nossas alterações.
    // ao contrário da suspeita normal que desce com o tempo, este valor é cumulativo e nunca desce, porque o que está arquivado fica arquivado
    private float companyAwareness = 0f;
    private const float AwarenessThresholdLow = 0.4f;
    private const float AwarenessThresholdHigh = 0.7f;

    private bool firedLowAlert = false;
    private bool firedHighAlert = false;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    // decidimos sacar um documento ao calhas em vez de ter uma ordem fixa para a jogabilidade ser menos previsível,
    // mas se calhar no futuro convém filtrar a dificuldade consoante o dia para os novatos não levarem com documentos lixados no dia 1
    public DocumentTaskData GetDocumentForToday() {
        return allDocuments[Random.Range(0, allDocuments.Length)];
    }

    // sempre que o jogador enfia um papel no arquivo, nós varremos os slots para somar os pesos das palavras que ele escolheu.
    // se o gajo se enganar no departamento aplicamos um multiplicador de 1.5x porque o documento vai parar às mãos erradas e dá mais cana
    public void ArchiveDocument(DocumentTaskData doc, ArchiveScript.DepartmentType department, bool correctDepartment) {

        float awarenessGain = 0f;

        foreach (BlankSlot blank in doc.blanks) {
            if (!choices.TryGetValue(blank.slotID, out string chosen))
                continue;

            bool correct = (chosen == blank.correctAnswer);

            float deptMultiplier = correctDepartment ? 1f : 1.5f;

            Debug.Log($"[DocumentManager] Slot '{blank.slotID}' arquivado em '{department}'" +
                      $" | Denúncia +{blank.weightDenuncia * deptMultiplier:F2}" +
                      $" | Extorsão +{blank.weightExtorsao * deptMultiplier:F2}" +
                      $" | Lealdade +{blank.weightLealdade * deptMultiplier:F2}");

            // fizemos o awareness escalar com o passar dos dias. normalizamos o valor para que os documentos no final da semana dêem muito mais estrondo que os primeiros
            awarenessGain += blank.awarenessWeight * deptMultiplier * (GameManager.Instance.currentDay / (float)GameManager.TotalDays);
        }

        // mesmo que o jogador não tenha alterado as lacunas, mandar papéis para o departamento errado gera sempre ruído interno
        if (!correctDepartment)
            awarenessGain += 0.05f;

        companyAwareness = Mathf.Clamp01(companyAwareness + awarenessGain);

        CheckAwarenessThresholds();

        Debug.Log($"[DocumentManager] Company Awareness: {companyAwareness:F2}");
    }

    // verifica se a empresa já desconfiou o suficiente para mudar o comportamento sistémico do jogo.
    // quando implementarmos o código disto, vai ligar aos NPCs e aos guardas
    private void CheckAwarenessThresholds() {

        if (!firedLowAlert && companyAwareness >= AwarenessThresholdLow) {
            firedLowAlert = true;
            Debug.Log("[DocumentManager] Company Awareness: sistema interno começou a cruzar dados.");
        }

        if (!firedHighAlert && companyAwareness >= AwarenessThresholdHigh) {
            firedHighAlert = true;
            Debug.Log("[DocumentManager] Company Awareness: alerta ativo - empresa sabe que algo está errado.");
        }
    }

    public void SaveChoice(string slotID, string chosenWord) {
        choices[slotID] = chosenWord;
        Debug.Log($"[DocumentManager] Slot '{slotID}' -> '{chosenWord}'");
    }

    public string GetChoice(string slotID) {
        return choices.TryGetValue(slotID, out string val) ? val : null;
    }

    public Dictionary<string, string> GetAllChoices() {
        return choices;
    }

    public float GetCompanyAwareness() {
        return companyAwareness;
    }

    public void SetCompanyAwareness(float value) {
        companyAwareness = value;
    }
}
