using UnityEngine;

// IntelReadUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntelReadUI : MonoBehaviour
{
    public static IntelReadUI Instance;

    [Header("Painel raiz")]
    public GameObject painelLeitura;

    [Header("Contedo")]
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtCategoria;
    public TextMeshProUGUI txtConteudo;

    [Header("Botes")]
    public Button btnGuardar;
    public Button btnIgnorar;

    private IntelItem itemAtual;
    private System.Action onGuardar;
    private System.Action onIgnorar;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        painelLeitura.SetActive(false);
    }


    /// Abre o painel com o item dado.
    /// callbackGuardar e callbackIgnorar so chamados quando o jogador decide.
    public void AbrirLeitura(IntelItem item, System.Action callbackGuardar, System.Action callbackIgnorar)
    {
        itemAtual = item;
        onGuardar = callbackGuardar;
        onIgnorar = callbackIgnorar;

        txtTitulo.text = item.titulo;
        txtCategoria.text = item.categoria.ToUpper();
        txtConteudo.text = item.conteudo;

        painelLeitura.SetActive(true);

        CameraScript.Instance.blockDetection = true;  // <-- bloqueia deteo
        UIManager.Instance.HideTooltip();
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        PlayerController.Instance.canMoveRotate = false;

        btnGuardar.onClick.RemoveAllListeners();
        btnIgnorar.onClick.RemoveAllListeners();
        btnGuardar.onClick.AddListener(Guardar);
        btnIgnorar.onClick.AddListener(Ignorar);
    }

    private void Fechar()
    {
        painelLeitura.SetActive(false);

        CameraScript.Instance.blockDetection = false;  // <-- restaura deteo
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;
    }
    private void Guardar()
    {
        IntelInventory.Instance.AdicionarIntel(itemAtual);
        Fechar();
        onGuardar?.Invoke();
    }

    private void Ignorar()
    {
        Fechar();
        onIgnorar?.Invoke();
    }

    
}