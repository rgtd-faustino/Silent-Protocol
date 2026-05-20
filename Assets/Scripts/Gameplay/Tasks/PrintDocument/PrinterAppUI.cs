using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Coloca este script no GameObject do canvas da impressora
// Ligaes necessrias no Inspector:
//   - noTaskPanel      -> painel com o texto "No h impresses a fazer"
//   - printPanel       -> painel com o nome do documento e o boto Imprimir
//   - confirmedPanel   -> painel que aparece depois de imprimir
//   - documentNameText -> TextMeshPro dentro do printPanel
//   - printerNameText  -> TextMeshPro dentro do confirmedPanel
//   - printButton      -> boto Imprimir dentro do printPanel

public class PrinterAppUI : MonoBehaviour
{

    [Header("Painis")]
    [SerializeField] private GameObject noTaskPanel;
    [SerializeField] private GameObject printPanel;
    [SerializeField] private GameObject confirmedPanel;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI documentNameText;
    [SerializeField] private TextMeshProUGUI printerNameText;

    [Header("Boto")]
    [SerializeField] private Button printButton;

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

            printButton.onClick.RemoveAllListeners();
            printButton.onClick.AddListener(OnPrintClicked);
        }
    }

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