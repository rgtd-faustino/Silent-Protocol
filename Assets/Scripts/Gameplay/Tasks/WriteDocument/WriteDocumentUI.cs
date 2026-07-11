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
    // deixamos o botão de submissão sempre ativo intencionalmente porque o jogador pode enviar um ficheiro com espaços em branco, o que aumenta a suspeita geral
    [SerializeField] private Button submitButton;

    [Header("Botões de escolha (4 botões fixos)")]
    [SerializeField] private Button[] choiceButtons;

    private DocumentTaskData currentData;

    private string[] chosenWords;
    // serve como controlo auxiliar para o loop do OnWordChosen saltar as perguntas que já têm resposta definida
    private bool[] filledSlots;
    // isto é o índice da lacuna que está a ser resolvida neste momento e usamos para saber onde colocar a tag amarela no texto gerado
    private int currentBlankIndex;

    // a ligação no OnEnable garante que consultamos o TaskManager no exato momento em que o PC é aberto
    // isto resolve o problema de dessincronização que tínhamos quando o script carregava logo no Start porque não apanhava nenhuma task
    void OnEnable() {
        // verificamos se existe alguma task
        bool hasMorning = TaskManager.Instance.HasActiveMorningTask("Escrever documento");
        bool hasAfternoon = TaskManager.Instance.HasActiveAfternoonTask("Escrever documento");

        if (hasMorning || hasAfternoon) {
            emptyStatePanel.SetActive(false);
            documentPanel.SetActive(true);
            // se houver então apanhamos os dados e mostramos a UI da tarefa
            OpenDocument(DocumentManager.Instance.GetDocumentForToday());

        } else {
            documentPanel.SetActive(false);
            emptyStatePanel.SetActive(true);
        }
    }

    private void OpenDocument(DocumentTaskData data) {
        // recolhemos as informações todas de cada texto, opções, etc
        currentData = data;
        chosenWords = new string[data.blanks.Length];
        filledSlots = new bool[data.blanks.Length];
        currentBlankIndex = 0;

        documentTitleText.text = data.documentTitle;

        RefreshBodyText();
        ShowOptionsForCurrentBlank();
    }

    // optámos por gerar a string toda de novo a cada clique
    // a nível de performance num texto pequeno é residual
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

    // limpa o painel e baralha as novas opções
    // ao fazer reset dos listeners dos botões evitamos comportamentos marados de cliques anteriores passarem para a próxima questão
    private void ShowOptionsForCurrentBlank() {
        foreach (Button btn in choiceButtons)
            btn.gameObject.SetActive(false);

        if (currentBlankIndex >= currentData.blanks.Length) 
            return;

        BlankSlot blank = currentData.blanks[currentBlankIndex];

        // adicionamos as opções e depois baralhamos para ser imprevisível
        List<string> options = new List<string>();
        options.Add(blank.correctAnswer);
        options.AddRange(blank.wrongOptions);
        Shuffle(options);

        for (int i = 0; i < options.Count && i < choiceButtons.Length; i++) {
            string word = options[i];

            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = word;

            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnWordChosen(word));
        }
    }

    // regista a escolha atual e empurra o índice para a frente para a próxima lacuna a preencher
    private void OnWordChosen(string word) {
        chosenWords[currentBlankIndex] = word;
        filledSlots[currentBlankIndex] = true;

        currentBlankIndex++;

        while (currentBlankIndex < filledSlots.Length && filledSlots[currentBlankIndex])
            currentBlankIndex++;

        RefreshBodyText();

        // enquanto todas as lacunas não forem preenchidas vamos mostrando as próximas opções, senão acaba a tarefa
        bool allFilled = currentBlankIndex >= currentData.blanks.Length;

        if (allFilled) {
            foreach (Button btn in choiceButtons)
                btn.gameObject.SetActive(false);

        } else {
            ShowOptionsForCurrentBlank();
        }
    }

    // confirma as escolhas finais e manda os dados para o DocumentManager que vai acumular os pesos das decisões narrativas
    // depois sinalizamos ao TaskManager para riscar a tarefa da lista
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