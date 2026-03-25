// BlankSlotUI.cs
using TMPro;
using UnityEngine;

public class BlankSlotUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI label;

    private int index;
    private bool filled = false;
    private bool correct = false;
    private string chosenWord = "";
    private WriteDocumentUI manager;

    public void Init(int idx, WriteDocumentUI mgr) {
        index = idx;
        manager = mgr;
        label.text = "______";
        label.color = Color.white;
    }

    public void Fill(string word, bool isCorrect) {
        filled = true;
        correct = isCorrect;
        chosenWord = word;
        label.text = word;
        label.color = isCorrect
            ? new Color(0.2f, 0.8f, 0.2f)   // verde
            : new Color(0.9f, 0.3f, 0.3f);  // vermelho
    }

    public bool IsFilled() => filled;
    public bool IsCorrect() => correct;
    public string GetChosenWord() => chosenWord;
}