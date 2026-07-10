using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrinterAppUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject noTaskPanel;
    [SerializeField] private GameObject printPanel;
    // Surge após o clique no botão de impressão para encaminhar o jogador para a impressora física correta.
    [SerializeField] private GameObject confirmedPanel;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI documentNameText;
    // Mostra o nome da impressora atribuída pelo TaskManager, funcionando como dica espacial.
    [SerializeField] private TextMeshProUGUI printerNameText;

    [Header("Botão")]
    [SerializeField] private Button printButton;

    // Preferimos o OnEnable em vez do Start para forçar uma verificação do estado da task sempre que o jogador abre o canvas da app.
    void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool taskActive = TaskManager.Instance.HasActiveTask("Imprimir documento");

        noTaskPanel.SetActive(!taskActive);
        printPanel.SetActive(taskActive);
        confirmedPanel.SetActive(false);

        if (taskActive)
        {
            DocumentTaskData doc = DocumentManager.Instance.GetDocumentForToday();
            documentNameText.text = $"Imprimir o documento: {doc.documentTitle}";
            printButton.interactable = true;

            // Limpamos os listeners antigos antes de adicionar um novo para evitar chamadas sobrepostas, visto que a interface pode ser reaberta várias vezes.
            printButton.onClick.RemoveAllListeners();
            printButton.onClick.AddListener(OnPrintClicked);
        }
    }

    // Comunica com o TaskManager para sortear uma impressora e ativá-la no mapa. A interface atualiza a seguir para dizer ao jogador onde tem de ir.
    private void OnPrintClicked()
    {
        printButton.interactable = false;

        ImpressoraScript chosenPrinter = TaskManager.Instance.ActivatePrinterTask();

        printPanel.SetActive(false);
        confirmedPanel.SetActive(true);

        string name = chosenPrinter != null ? chosenPrinter.objectName : "Impressora";
        printerNameText.text = $"Foi impresso na impressora: {name}";
    }
}