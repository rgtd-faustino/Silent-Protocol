using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelInventory : MonoBehaviour
{
    public static IntelInventory Instance;

    // ------------------------------------------------------------------ //
    // Referências UI                                                        //
    // ------------------------------------------------------------------ //

    [Header("Painel Raiz")]
    public GameObject dossierPanel;

    [Header("Sidebar")]
    public TextMeshProUGUI txtTotalIntel;
    public Button btnFiltroTodos;
    public Button btnFiltroCredenciais;
    public Button btnFiltroLocalizacao;
    public Button btnFiltroContatos;

    [Header("Lista")]
    public Transform entryListContent;
    public GameObject entryButtonPrefab;

    [Header("Detalhe")]
    public GameObject intelDetailPanel;
    public GameObject painelVazio;
    public GameObject badgeCategoria;
    public TextMeshProUGUI txtBadgeCategoria;
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtLocalizacao;
    public TextMeshProUGUI txtCategoria;
    public TextMeshProUGUI txtConteudo;

    [Header("Notificação (opcional — badge no HUD)")]
    public GameObject badgeNovoIntel;
    public TextMeshProUGUI txtBadgeNovoIntel;

    // ------------------------------------------------------------------ //
    // Estado interno                                                        //
    // ------------------------------------------------------------------ //

    private List<IntelItem> entradas = new List<IntelItem>();
    private List<GameObject> entradasAtivas = new List<GameObject>();
    private IntelItem itemSelecionado = null;
    private string filtroAtual = "Todos";
    private int novosNaoVistos = 0;

    // cores por categoria para o badge (podes expandir)
    private readonly Dictionary<string, string> coresBadge = new Dictionary<string, string>()
    {
        { "Credenciais", "#0F6E56" },
        { "Localização",  "#185FA5" },
        { "Contactos",    "#993556" },
        { "Outros",       "#5F5E5A" },
    };

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // botões de filtro
        btnFiltroTodos.onClick.AddListener(() => AplicarFiltro("Todos"));
        btnFiltroCredenciais.onClick.AddListener(() => AplicarFiltro("Credenciais"));
        btnFiltroLocalizacao.onClick.AddListener(() => AplicarFiltro("Localização"));
        btnFiltroContatos.onClick.AddListener(() => AplicarFiltro("Contactos"));

        // estado inicial
        intelDetailPanel.SetActive(false);
        if (painelVazio != null) painelVazio.SetActive(true);
        AtualizarBadgeNovoIntel();
        AtualizarTotalIntel();
    }

   

    // ------------------------------------------------------------------ //
    // API pública                                                           //
    // ------------------------------------------------------------------ //

    public void ToggleDossier()
    {
        bool abrir = !dossierPanel.activeSelf;
        dossierPanel.SetActive(abrir);

        if (abrir)
        {
            // reset badge de novos ao abrir
            novosNaoVistos = 0;
            AtualizarBadgeNovoIntel();

            UIManager.Instance.ChangeCursorState(CursorLockMode.None);
            PlayerController.Instance.canMoveRotate = false;
            AtualizarLista();
        }
        else
        {
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
        }
    }

    /// <summary>
    /// Adiciona um IntelItem ao inventário. Chamado pelo EmailUI ou por qualquer outro sistema.
    /// </summary>
    public void AdicionarIntel(IntelItem item)
    {
        if (entradas.Contains(item)) return;

        entradas.Add(item);
        novosNaoVistos++;

        AtualizarBadgeNovoIntel();
        AtualizarTotalIntel();

        // se o dossier estiver aberto atualiza a lista em direto
        if (dossierPanel.activeSelf)
            AtualizarLista();
    }

    public int GetTotalIntel() => entradas.Count;

    // ------------------------------------------------------------------ //
    // Filtro e Lista                                                        //
    // ------------------------------------------------------------------ //

    private void AplicarFiltro(string filtro)
    {
        filtroAtual = filtro;
        itemSelecionado = null;
        intelDetailPanel.SetActive(false);
        if (painelVazio != null) painelVazio.SetActive(true);
        AtualizarLista();
        AtualizarEstadoBotoesFiltro();
    }

    private void AtualizarLista()
    {
        foreach (var go in entradasAtivas) Destroy(go);
        entradasAtivas.Clear();

        foreach (var item in entradas)
        {
            if (filtroAtual != "Todos" && item.categoria != filtroAtual) continue;
            CriarEntrada(item);
        }
    }

    private void CriarEntrada(IntelItem item)
    {
        var go = Instantiate(entryButtonPrefab, entryListContent);
        entradasAtivas.Add(go);

        var labels = go.GetComponentsInChildren<TextMeshProUGUI>();
        if (labels.Length >= 1) labels[0].text = item.titulo;
        if (labels.Length >= 2) labels[1].text = item.categoria;

        go.GetComponent<Button>().onClick.AddListener(() => MostrarDetalhe(item));
    }

    // ------------------------------------------------------------------ //
    // Detalhe                                                               //
    // ------------------------------------------------------------------ //

    private void MostrarDetalhe(IntelItem item)
    {
        itemSelecionado = item;

        txtTitulo.text = item.titulo;
        txtLocalizacao.text = item.localizacao;
        txtCategoria.text = item.categoria;
        txtConteudo.text = item.conteudo;

        // badge de categoria com cor
        if (badgeCategoria != null && txtBadgeCategoria != null)
        {
            txtBadgeCategoria.text = item.categoria.ToUpper();
            badgeCategoria.SetActive(!string.IsNullOrEmpty(item.categoria));
        }

        if (painelVazio != null) painelVazio.SetActive(false);
        intelDetailPanel.SetActive(true);
    }

    // ------------------------------------------------------------------ //
    // Helpers visuais                                                       //
    // ------------------------------------------------------------------ //

    private void AtualizarTotalIntel()
    {
        if (txtTotalIntel != null)
            txtTotalIntel.text = $"{entradas.Count} INTEL RECOLHIDA";
    }

    private void AtualizarBadgeNovoIntel()
    {
        if (badgeNovoIntel == null) return;
        badgeNovoIntel.SetActive(novosNaoVistos > 0);
        if (txtBadgeNovoIntel != null)
            txtBadgeNovoIntel.text = novosNaoVistos.ToString();
    }

    private void AtualizarEstadoBotoesFiltro()
    {
        // feedback visual de qual filtro está ativo
        // podes ligar a cores/imagens no Inspector se quiseres highlight
        // por agora só um log para confirmar
        Debug.Log($"[IntelInventory] Filtro ativo: {filtroAtual}");
    }

    [Header("Save System")]
    public IntelItem[] allPossibleIntel;

    public List<string> GetCollectedIntelNames() {
        List<string> names = new List<string>();
        foreach (var item in entradas) {
            names.Add(item.name);
        }
        return names;
    }

    public void RestoreIntelFromNames(List<string> names) {
        entradas.Clear();
        if (names == null) return;
        foreach (string n in names) {
            foreach (var possible in allPossibleIntel) {
                if (possible.name == n) {
                    entradas.Add(possible);
                    break;
                }
            }
        }
        AtualizarTotalIntel();
    }
}
