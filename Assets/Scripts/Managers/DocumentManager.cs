using System.Collections.Generic;
using UnityEngine;

public class DocumentManager : MonoBehaviour {

    public static DocumentManager Instance;

    [SerializeField] private DocumentTaskData[] allDocuments;

    // chave = slotID da lacuna, valor = palavra escolhida pelo jogador
    private Dictionary<string, string> choices = new Dictionary<string, string>();

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Documentos ---

    public DocumentTaskData GetDocumentForToday() {
        return allDocuments[Random.Range(0, allDocuments.Length)];
    }

    // --- Escolhas do jogador ---

    public void SaveChoice(string slotID, string chosenWord) {
        choices[slotID] = chosenWord;
        Debug.Log($"[DocumentManager] Slot '{slotID}' → '{chosenWord}'");
    }

    public string GetChoice(string slotID) =>
        choices.TryGetValue(slotID, out string val) ? val : null;

    public Dictionary<string, string> GetAllChoices() => choices;
}