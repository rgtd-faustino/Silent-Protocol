using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntelReadUI : MonoBehaviour
{
    public static IntelReadUI Instance;

    [Header("Painel raiz")]
    public GameObject painelLeitura;

    [Header("Conteudo")]
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtCategoria;
    public TextMeshProUGUI txtConteudo;

    [Header("Botoes")]
    public Button btnGuardar;
    public Button btnIgnorar;

    private IntelItem itemAtual;
    private System.Action onGuardar;
    private System.Action onIgnorar;

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        painelLeitura.SetActive(false);
    }

    // abre o painel e tranca o jogo todo para o jogador decidir o que fazer com a informação
    public void AbrirLeitura(IntelItem item, System.Action callbackGuardar, System.Action callbackIgnorar)
    {
        itemAtual = item;
        onGuardar = callbackGuardar;
        onIgnorar = callbackIgnorar;

        txtTitulo.text = item.titulo;
        txtCategoria.text = item.categoria.ToUpper();
        txtConteudo.text = item.conteudo;

        painelLeitura.SetActive(true);

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