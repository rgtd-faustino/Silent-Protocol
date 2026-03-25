using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WriteDocumentUI : MonoBehaviour {

    [Header("Painéis")]
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private GameObject emptyStatePanel;

    [Header("UI do documento")]
    [SerializeField] private TextMeshProUGUI documentTitleText;
    [SerializeField] private Transform documentBodyParent;
    [SerializeField] private Transform wordChoicesParent;
    [SerializeField] private Button submitButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject textSegmentPrefab;
    [SerializeField] private GameObject blankSlotPrefab;
    [SerializeField] private GameObject wordChoicePrefab;

    private DocumentTaskData currentData;
    private BlankSlotUI[] blankSlots;
    private int filledCount = 0;


    void OnEnable() {
        bool hasMorning = TaskManager.Instance.HasActiveMorningTask("Escrever documento");
        bool hasAfternoon = TaskManager.Instance.HasActiveAfternoonTask("Escrever documento");

        if (hasMorning || hasAfternoon) {
            emptyStatePanel.SetActive(false);
            documentPanel.SetActive(true);
            OpenDocument(DocumentManager.Instance.GetDocumentForToday()); // pede ao manager
        } else {
            documentPanel.SetActive(false);
            emptyStatePanel.SetActive(true);
        }
    }


    private void OpenDocument(DocumentTaskData data) {
        currentData = data;
        filledCount = 0;
        submitButton.interactable = false;
        documentTitleText.text = data.documentTitle;
        BuildBody(data);
        BuildChoices(data);
        // sem SetActive aqui — já foi feito no OnEnable
    }


    private void BuildBody(DocumentTaskData data) {
        foreach (Transform child in documentBodyParent)
            Destroy(child.gameObject);

        string[] parts = System.Text.RegularExpressions.Regex
            .Split(data.bodyText, @"(\{\d+\})");

        blankSlots = new BlankSlotUI[data.blanks.Length];

        foreach (string part in parts) {
            var match = System.Text.RegularExpressions.Regex
                .Match(part, @"\{(\d+)\}");

            if (match.Success) {
                int index = int.Parse(match.Groups[1].Value);
                GameObject go = Instantiate(blankSlotPrefab, documentBodyParent);
                BlankSlotUI slot = go.GetComponent<BlankSlotUI>();
                slot.Init(index, this);
                blankSlots[index] = slot;
            } else if (!string.IsNullOrEmpty(part)) {
                GameObject go = Instantiate(textSegmentPrefab, documentBodyParent);
                go.GetComponent<TextMeshProUGUI>().text = part;
            }
        }
    }


    private void BuildChoices(DocumentTaskData data) {
        foreach (Transform child in wordChoicesParent)
            Destroy(child.gameObject);

        var allOptions = new List<(string word, int blankIndex, bool isCorrect)>();

        for (int i = 0; i < data.blanks.Length; i++) {
            allOptions.Add((data.blanks[i].correctAnswer, i, true));
            foreach (string wrong in data.blanks[i].wrongOptions)
                allOptions.Add((wrong, i, false));
        }

        for (int i = allOptions.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (allOptions[i], allOptions[j]) = (allOptions[j], allOptions[i]);
        }

        foreach (var option in allOptions) {
            GameObject go = Instantiate(wordChoicePrefab, wordChoicesParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = option.word;

            string w = option.word;
            int idx = option.blankIndex;
            bool correct = option.isCorrect;

            go.GetComponent<Button>().onClick.AddListener(() =>
                OnWordChosen(w, idx, correct));
        }
    }


    public void OnWordChosen(string word, int blankIndex, bool isCorrect) {
        if (blankSlots[blankIndex].IsFilled()) return;
        blankSlots[blankIndex].Fill(word, isCorrect);
        filledCount++;
        if (filledCount >= currentData.blanks.Length)
            submitButton.interactable = true;
    }


    public void OnSubmit() {
        bool allCorrect = true;

        for (int i = 0; i < blankSlots.Length; i++) {
            if (!blankSlots[i].IsCorrect()) allCorrect = false;
            DocumentManager.Instance.SaveChoice(
                currentData.blanks[i].slotID,
                blankSlots[i].GetChosenWord()
            );
        }

        TaskManager.Instance.CompleteTask("Escrever documento", allCorrect);
        gameObject.SetActive(false);
    }
}