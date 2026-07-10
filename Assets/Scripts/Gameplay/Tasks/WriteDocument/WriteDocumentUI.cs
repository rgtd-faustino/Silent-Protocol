using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static DocumentTaskData;

public class WriteDocumentUI : MonoBehaviour {

    [Header("Painéis")]
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private GameObject emptyStatePanel;

    [Header("UI do documento")]
    [SerializeField] private TextMeshProUGUI documentTitleText;
    [SerializeField] private TextMeshProUGUI documentBodyText;
    // Deixamos o botão de submissão sempre ativo intencionalmente. O jogador pode enviar um ficheiro com espaços em branco, o que desencadeia penalizações no SuspicionManager.
    [SerializeField] private Button submitButton;

    [Header("Botões de escolha (4 botões fixos)")]
    [SerializeField] private Button[] choiceButtons;

    private DocumentTaskData currentData;

    private string[] chosenWords;
    // Serve como controlo auxiliar para o loop do OnWordChosen saltar as perguntas que já têm resposta definida.
    private bool[] filledSlots;
    // O índice da lacuna que está a ser resolvida neste momento. Usamos isto para saber onde colocar a tag amarela no texto gerado.
    private int currentBlankIndex;

    // A ligação no OnEnable garante que consultamos o TaskManager no exato momento em que o PC é aberto. Isto resolve o problema de dessincronização que tínhamos quando o script carregava logo no Start.
    void OnEnable() {
        bool hasMorning = TaskManager.Instance.HasActiveMorningTask("Escrever documento");
        bool hasAfternoon = TaskManager.Instance.HasActiveAfternoonTask("Escrever documento");

        if (hasMorning || hasAfternoon) {
            emptyStatePanel.SetActive(false);
            documentPanel.SetActive(true);
            OpenDocument(DocumentManager.Instance.GetDocumentForToday());

        } else {
            documentPanel.SetActive(false);
            emptyStatePanel.SetActive(true);
        }
    }

    private void OpenDocument(DocumentTaskData data) {
        currentData = data;
        chosenWords = new string[data.blanks.Length];
        filledSlots = new bool[data.blanks.Length];
        currentBlankIndex = 0;

        documentTitleText.text = data.documentTitle;

        RefreshBodyText();
        ShowOptionsForCurrentBlank();
    }

    // Optámos por gerar a string toda de novo a cada clique. A nível de performance num texto pequeno é residual, e poupa-nos a imensa chatice de gerir as tags de rich text manualmente via código.
    private void RefreshBodyText() {
        string body = currentData.bodyText;

        for (int i = 0; i < currentData.blanks.Length; i++) {
            string placeholder = "{" + i + "}";

            if (filledSlots[i])
                body = body.Replace(placeholder, $"<color=#ffffff><u>{chosenWords[i]}</u></color>");
            else if (i == currentBlankIndex)
                body = body.Replace(placeholder, "<color=#FFD700><u>______</u></color>");
            else
                body = body.Replace(placeholder, "______");
        }

        documentBodyText.text = body;
    }

    // Limpa o painel e baralha as novas opções. Ao fazer reset dos listeners dos botões evitamos comportamentos marados de cliques anteriores passarem para a próxima questão.
    private void ShowOptionsForCurrentBlank() {
        foreach (Button btn in choiceButtons)
            btn.gameObject.SetActive(false);

        if (currentBlankIndex >= currentData.blanks.Length) 
            return;

        BlankSlot blank = currentData.blanks[currentBlankIndex];

        List<string> options = new List<string>();
        options.Add(blank.correctAnswer);
        options.AddRange(blank.wrongOptions);
        Shuffle(options);

        for (int i = 0; i < options.Count && i < choiceButtons.Length; i++) {
            string word = options[i];
            bool isCorrect = (word == blank.correctAnswer);

            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = word;

            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnWordChosen(word, isCorrect));
        }
    }

    // Regista a escolha atual e empurra o índice para a frente. O ciclo while protege-nos caso um dia implementemos navegação livre pelas respostas e o jogador responda fora de ordem.
    private void OnWordChosen(string word, bool isCorrect) {
        chosenWords[currentBlankIndex] = word;
        filledSlots[currentBlankIndex] = true;

        currentBlankIndex++;

        while (currentBlankIndex < filledSlots.Length && filledSlots[currentBlankIndex])
            currentBlankIndex++;

        RefreshBodyText();

        bool allFilled = currentBlankIndex >= currentData.blanks.Length;

        if (allFilled) {
            foreach (Button btn in choiceButtons)
                btn.gameObject.SetActive(false);
        } else {
            ShowOptionsForCurrentBlank();
        }
    }

    // Confirma as escolhas finais e empurra os dados para o DocumentManager, que vai tratar de acumular os pesos das decisões narrativas. Depois sinalizamos ao TaskManager para riscar a tarefa da lista.
    public void OnSubmit() {
        bool allFilled = true;

        for (int i = 0; i < currentData.blanks.Length; i++) {
            if (string.IsNullOrEmpty(chosenWords[i])) {
                allFilled = false;
            }

            DocumentManager.Instance.SaveChoice(currentData.blanks[i].slotID, chosenWords[i] ?? "");
        }

        TaskManager.Instance.CompleteTask("Escrever documento", allFilled);
        gameObject.SetActive(false);
    }

    // Substitui o algoritmo tradicional de Fisher-Yates por um loop simples de trocas aleatórias. Como só temos quatro opções no máximo, a diferença na distribuição é negligenciável e o código fica super fácil de ler.
    private void Shuffle(List<string> list) {
        for (int i = 0; i < list.Count * 2; i++) {
            int a = Random.Range(0, list.Count);
            int b = Random.Range(0, list.Count);
            string temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }
    }
}