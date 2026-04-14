using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalUI : MonoBehaviour
{

    [Header("Output")]
    [SerializeField] private ScrollRect outputScroll;
    [SerializeField] private Transform outputContent;
    [SerializeField] private GameObject terminalLinePrefab;

    [Header("Input")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Button enterButton;

    // cores do terminal
    private static readonly Color ColSys = new Color(0.10f, 0.41f, 0.10f);
    private static readonly Color ColInfo = new Color(0.18f, 0.62f, 0.18f);
    private static readonly Color ColInput = new Color(0.31f, 0.79f, 0.31f);
    private static readonly Color ColHex = new Color(0.66f, 1.00f, 0.66f);
    private static readonly Color ColHash = new Color(1.00f, 0.87f, 0.27f);
    private static readonly Color ColPlain = new Color(0.88f, 1.00f, 0.88f);
    private static readonly Color ColErr = new Color(1.00f, 0.27f, 0.27f);
    private static readonly Color ColDim = new Color(0.12f, 0.29f, 0.12f);
    private static readonly Color ColSep = new Color(0.10f, 0.23f, 0.10f);
    private static readonly Color ColPrompt = new Color(1.00f, 0.87f, 0.27f);
    private static readonly Color ColSaved = new Color(0.16f, 0.78f, 0.25f);

    public enum LineType { Sys, Info, Input, Hex, Hash, Plain, Err, Dim, Sep, Prompt, Saved }

    // referência ao manager para passar comandos
    private TerminalManager manager;

    void Start()
    {
        manager = GetComponent<TerminalManager>();
        enterButton.onClick.AddListener(OnEnterPressed);
        inputField.onSubmit.AddListener(_ => OnEnterPressed());

        ClearOutput();
        PrintBoot();
        FocusInput();
    }

    void OnEnable()
    {
        // só faz boot se já foi inicializado
        if (outputContent == null) return;
        ClearOutput();
        PrintBoot();
        FocusInput();
    }
    // mensagem de arranque
    private void PrintBoot()
    {
        AddLine("╔══════════════════════════════════════════╗", LineType.Sys);
        AddLine("║        CRYPTER TERMINAL  v1.0            ║", LineType.Sys);
        AddLine("╚══════════════════════════════════════════╝", LineType.Sys);
        AddLine(" ", LineType.Dim);
        AddLine("Sistema inicializado.", LineType.Info);
        AddLine("Aguarda input do utilizador.", LineType.Dim);
        AddLine(" ", LineType.Dim);
        AddLine("──────────────────────────────────────────", LineType.Sep);
        AddLine(" ", LineType.Dim);
    }


    // --- API pública — o TerminalManager chama estes métodos ---

    public void AddLine(string text, LineType type)
    {
        GameObject obj = Instantiate(terminalLinePrefab, outputContent);
        TerminalLineUI line = obj.GetComponent<TerminalLineUI>();
        line.SetLine(text, GetColor(type));
        StartCoroutine(ScrollToBottom());
    }

    public void AddBlank() => AddLine(" ", LineType.Dim);

    // muda o texto do prompt (ex: ">" durante perguntas S/N, "titulo >" durante rename)
    public void SetPrompt(string text)
    {
        promptText.text = text;
    }

    public void ClearOutput()
    {
        foreach (Transform child in outputContent)
            Destroy(child.gameObject);
    }

    public void FocusInput()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void LockInput(bool locked)
    {
        inputField.interactable = !locked;
        enterButton.interactable = !locked;
    }

    // lê e limpa o input — chamado pelo manager após processar
    public string ConsumeInput()
    {
        string val = inputField.text.Trim();
        inputField.text = "";
        return val;
    }

    public string GetCurrentPrompt() => promptText.text;


    // --- interno ---

    private void OnEnterPressed()
    {
        string val = inputField.text.Trim();
        if (string.IsNullOrEmpty(val)) return;
        manager?.HandleInput(val);
        inputField.text = "";
        FocusInput();
    }

    private IEnumerator ScrollToBottom()
    {
        // espera um frame para o layout recalcular antes de fazer scroll
        yield return null;
        yield return null;
        outputScroll.verticalNormalizedPosition = 0f;
    }

    private Color GetColor(LineType type)
    {
        return type switch
        {
            LineType.Sys => ColSys,
            LineType.Info => ColInfo,
            LineType.Input => ColInput,
            LineType.Hex => ColHex,
            LineType.Hash => ColHash,
            LineType.Plain => ColPlain,
            LineType.Err => ColErr,
            LineType.Dim => ColDim,
            LineType.Sep => ColSep,
            LineType.Prompt => ColPrompt,
            LineType.Saved => ColSaved,
            _ => ColDim
        };
    }
}