using System.Collections.Generic;
using UnityEngine;

public class DocumentManager : MonoBehaviour {

    public static DocumentManager Instance;

    [SerializeField] private DocumentTaskData[] allDocuments;

    // dicionário que mapeia slotID → palavra escolhida pelo jogador
    private Dictionary<string, string> choices = new Dictionary<string, string>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // --- Documentos ---

    // retorna um documento aleatório do pool —> chamado pelo WriteDocumentUI quando o painel é aberto a aleatoriedade mantém cada sessão imprevisível
    // fazer -> documento mais simples no dia 1, mais comprometedor no dia 5
    public DocumentTaskData GetDocumentForToday() {
        return allDocuments[Random.Range(0, allDocuments.Length)];
    }


    // --- Escolhas do jogador ---

    // guarda a palavra escolhida para um dado slotID —> chamado pelo WriteDocumentUI no OnSubmit, uma vez por lacuna
    public void SaveChoice(string slotID, string chosenWord) {
        choices[slotID] = chosenWord;
        Debug.Log($"[DocumentManager] Slot '{slotID}' → '{chosenWord}'");
    }

    // leitura de uma escolha específica —> útil para sistemas futuros que precisem de saber o que o jogador escreveu num slot concreto sem ter acesso a todo o dicionário
    // (ex: um NPC que comente uma decisão específica do jogador num documento anterior)
    public string GetChoice(string slotID) => choices.TryGetValue(slotID, out string val) ? val : null;

    // devolve o dicionário completo —> usado pelo sistema de finais para calcular para que ending o perfil de escolhas do jogador aponta
    // somando os weightDenuncia, weightExtorsao, weightLealdade de cada BlankSlot correspondente aos slotIDs aqui guardados
    public Dictionary<string, string> GetAllChoices() => choices;
}