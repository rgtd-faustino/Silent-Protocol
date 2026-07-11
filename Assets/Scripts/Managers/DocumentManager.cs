using System.Collections.Generic;
using UnityEngine;
using static DocumentTaskData;

public class DocumentManager : MonoBehaviour
{

    public static DocumentManager Instance;

    // guardamos tods os documentos que existem para as tarefas diárias
    [SerializeField] private DocumentTaskData[] allDocuments;

    // guarda as opções que o jogador meteu nas lacunas dos documentos (slotID para a palavra)
    // acumulamos isto durante a semana e no final o jogo usa este dicionário para calcular o perfil do jogador e o ending
    private Dictionary<string, string> choices = new Dictionary<string, string>();

    // isto representa o nível de alerta passivo da empresa perante as nossas alterações
    // ao contrário da suspeita normal que desce com o tempo, este valor é cumulativo e nunca desce, porque o que está arquivado fica arquivado
    private float companyAwareness = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // decidimos apanhar um documento ao calhas em vez de ter uma ordem fixa para a jogabilidade ser menos previsível
    // mas se calhar no futuro convém filtrar a dificuldade consoante o dia
    public DocumentTaskData GetDocumentForToday()
    {
        return allDocuments[Random.Range(0, allDocuments.Length)];
    }

    // sempre que o jogador arquiva um papel percorremos os slots para somar os pesos das palavras que ele escolheu
    // se o jogador se enganar no departamento aplicamos um multiplicador de 1.5x porque o documento vai parar às mãos erradas
    public void ArchiveDocument(DocumentTaskData doc, ArchiveScript.DepartmentType department, bool correctDepartment)
    {
        float awarenessGain = 0f;

        foreach (BlankSlot blank in doc.blanks)
        {
            if (!choices.TryGetValue(blank.slotID, out string chosen))
                continue;

            bool correct = (chosen == blank.correctAnswer);

            float deptMultiplier = correctDepartment ? 1f : 1.5f;

            // fizemos o awareness escalar com o passar dos dias, normalizamos o valor para que os documentos no final da semana dêem muito mais impacto que os primeiros
            // isto funciona como uma espécie de nível de dificuldade para os dias iniciais serem mais calmos e não gerem tantas consequências
            awarenessGain += blank.awarenessWeight * deptMultiplier * (GameManager.Instance.currentDay / (float)GameManager.TotalDays);
        }

        // mesmo que o jogador não tenha alterado as lacunas, mandar papéis para o departamento errado gera sempre ruído interno
        if (!correctDepartment)
            awarenessGain += 0.05f;

        companyAwareness = Mathf.Clamp01(companyAwareness + awarenessGain);

        Debug.Log($"[DocumentManager] Company Awareness: {companyAwareness:F2}");
    }

    public void SaveChoice(string slotID, string chosenWord)
    {
        choices[slotID] = chosenWord;
        Debug.Log($"[DocumentManager] Slot '{slotID}' -> '{chosenWord}'");
    }

    public string GetChoice(string slotID)
    {
        return choices.TryGetValue(slotID, out string val) ? val : null;
    }

    public Dictionary<string, string> GetAllChoices()
    {
        return choices;
    }

    public float GetCompanyAwareness()
    {
        return companyAwareness;
    }

    public void SetCompanyAwareness(float value)
    {
        companyAwareness = value;
    }

    // limpa as escolhas nas lacunas dos documentos, o company awareness e os alertas já disparados, para que um "Novo Jogo" comece do zero
    public void ResetForNewGame()
    {
        choices.Clear();
        companyAwareness = 0f;

        Debug.Log("[DocumentManager] Estado reiniciado para um novo jogo.");
    }
}