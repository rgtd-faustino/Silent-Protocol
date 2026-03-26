using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static DocumentTaskData;

public class WriteDocumentUI : MonoBehaviour {

    [Header("Painéis")]
    // documentPanel é o estado normal —> mostra o documento a preencher
    // emptyStatePanel é para quando o jogador abre o computador mas não tem esta task ativa
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private GameObject emptyStatePanel;

    [Header("UI do documento")]
    [SerializeField] private TextMeshProUGUI documentTitleText;
    [SerializeField] private TextMeshProUGUI documentBodyText;
    // submitButton está sempre ativo —> se o jogador submeter com lacunas por preencher, a task conta como mal feita e a suspeita sobe
    [SerializeField] private Button submitButton;

    [Header("Botões de escolha (4 botões fixos)")]
    [SerializeField] private Button[] choiceButtons;

    // referência ao ScriptableObject do documento ativo
    private DocumentTaskData currentData;

    private string[] chosenWords; // chosenWords guarda a palavra que o jogador escolheu para cada lacuna, indexado por posição no array blanks
    private bool[] filledSlots; // filledSlots marca quais lacunas já foram respondidas

    // qual a lacuna que está ativa no momento —> determina qual conjunto de opções mostrar e qual placeholder fica destacado a amarelo
    private int currentBlankIndex;


    // OnEnable em vez de Start porque este GameObject é ativado/desativado pelo PCInteractable cada vez que o PC é aberto ou seja
    // o OnEnable corre de novo sempre que o painel fica visível, assim o estado das tasks é sempre verificado
    // no momento certo e não com dados desatualizados de quando o objeto foi criado pela primeira vez
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


    // inicializa todos os arrays e estado local com base no documento recebido
    private void OpenDocument(DocumentTaskData data) {
        currentData = data;
        chosenWords = new string[data.blanks.Length];
        filledSlots = new bool[data.blanks.Length];
        currentBlankIndex = 0;

        documentTitleText.text = data.documentTitle;

        RefreshBodyText();
        ShowOptionsForCurrentBlank();
    }


    // reconstrói o texto completo do documento de raiz a cada escolha
    // a cor amarela na lacuna ativa serve para o jogador saber sempre onde está a escolher
    // a cor branca nas lacunas já preenchidas (e o sublinhado) distingue-as das lacunas por preencher (traço simples sem cor)
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


    // esconde todos os botões antes de mostrar os da lacuna atual
    // as opções são baralhadas para que a resposta correta não esteja sempre na mesma posição
    // RemoveAllListeners antes de AddListener porque o mesmo botão é reutilizado entre lacunas
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


    // chamado quando o jogador clica numa opção, avança o estado interno para a próxima lacuna por preencher
    // quando todas as lacunas estão preenchidas, os botões desaparecem
    // COMO NÃO HÁ RESPOSTAS ERRADAS PODEMOS FAZER QUE SE UM CHEFE CALHAR A VER UM DOCUMENTO COM RESPOSTAS "erradas" ENTÃO O NIVEL DE SUSPEITA/company awareness AUMENTA
    // ASSIM O JOGADOR É CASTIGADO POR METER RESPOSTAS QUE NAO SEJAM CORRETAS
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


    // ao submeter, percorre todas as lacunas para determinar se a tarefa foi bem feita
    // todas as lacunas preenchidas = tarefa correta, lacunas por preencher = tarefa mal feita, suspeita sobe
    // as escolhas são guardadas no DocumentManager para os pesos narrativos (weightDenuncia, etc.) que calculam para que final o jogador está a caminhar
    public void OnSubmit() {
        // correto = todas as lacunas preenchidas (o jogador fez o trabalho)
        // as escolhas afetam pesos narrativos, não a avaliação da task
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


    // algoritmo simples para baralhar as opções para que a resposta correta não esteja sempre na mesma posição
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