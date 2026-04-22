using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Server2UI : MonoBehaviour
{
    public static Server2UI Instance;

    [Header("Painel")]
    [SerializeField] private GameObject serverPanel;

    [Header("Código")]
    [SerializeField] private TMP_InputField[] codeDigits;
    [SerializeField] private TextMeshProUGUI codeResultText;

    [Header("Intel")]
    [SerializeField] private IntelItem srv2Intel;
    private bool intelDado = false;

    private const string SRV2_CODE = "34226";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        serverPanel.SetActive(false);
    }

    public void OnOpen()
    {
        codeResultText.text = "";
        foreach (var d in codeDigits) d.text = "";
    }

    public void CheckCode()
    {
        string code = "";
        foreach (var d in codeDigits) code += d.text;

        if (code == SRV2_CODE)
        {
            codeResultText.text = "ACCESS GRANTED";
            codeResultText.color = Color.green;

            if (!intelDado && srv2Intel != null)
            {
                intelDado = true;
                IntelInventory.Instance.AdicionarIntel(srv2Intel);
            }
        }
        else
        {
            codeResultText.text = "ACCESS DENIED";
            codeResultText.color = Color.red;
        }
    }
}