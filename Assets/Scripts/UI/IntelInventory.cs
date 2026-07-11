using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelInventory : MonoBehaviour
{
    public static IntelInventory Instance;

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

    [Header("Notificacao")]
    public GameObject badgeNovoIntel;
    public TextMeshProUGUI txtBadgeNovoIntel;

    private List<IntelItem> entradas = new List<IntelItem>();
    private List<GameObject> entradasAtivas = new List<GameObject>();
    private IntelItem itemSelecionado = null;
    private string filtroAtual = "Todos";

    // Rastreamos quantos itens ainda não foram vistos para poder colocar a notificação na interface
    private int novosNaoVistos = 0;

    private readonly Dictionary<string, string> coresBadge = new Dictionary<string, string>()
    {
        { "Credenciais", "#0F6E56" },
        { "Localização",  "#185FA5" },
        { "Contactos",    "#993556" },
        { "Outros",       "#5F5E5A" },
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        btnFiltroTodos.onClick.AddListener(() => AplicarFiltro("Todos"));
        btnFiltroCredenciais.onClick.AddListener(() => AplicarFiltro("Credenciais"));
        btnFiltroLocalizacao.onClick.AddListener(() => AplicarFiltro("Localização"));
        btnFiltroContatos.onClick.AddListener(() => AplicarFiltro("Contactos"));

        intelDetailPanel.SetActive(false);
        if (painelVazio != null) painelVazio.SetActive(true);
        AtualizarBadgeNovoIntel();
        AtualizarTotalIntel();
    }

    // Abre e fecha o UI do dossier e gere o input do jogador
    // Notificamos o TutorialManager caso estejamos a passar a fase de aprender a usar o dossier
    public void ToggleDossier()
    {
        bool abrir = !dossierPanel.activeSelf;
        dossierPanel.SetActive(abrir);

        if (abrir)
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_dossier"))
            {
                TutorialManager.Instance.CompleteCurrentStep();
            }

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

    // Adiciona o item apanhado à lista e atualiza as contagens
    // Chamado externamente quando o jogador lê emails ou apanha ficheiros físicos
    public void AdicionarIntel(IntelItem item)
    {
        if (entradas.Contains(item)) return;

        entradas.Add(item);
        novosNaoVistos++;

        AtualizarBadgeNovoIntel();
        AtualizarTotalIntel();

        if (dossierPanel.activeSelf)
            AtualizarLista();
    }

    public int GetTotalIntel() => entradas.Count;

    // Calcula a soma do valor das peças de informação recolhidas
    // Esta percentagem será usada pelo GameManager para determinar o final do jogo
    public float GetTotalPercentage()
    {
        float total = 0f;
        foreach (var item in entradas)
            total += item.percentagemContribuicao;

        return Mathf.Clamp(total, 0f, 100f);
    }

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

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            MostrarDetalhe(item);
        });
    }

    private void MostrarDetalhe(IntelItem item)
    {
        itemSelecionado = item;

        txtTitulo.text = item.titulo;
        txtLocalizacao.text = item.localizacao;
        txtCategoria.text = item.categoria;
        txtConteudo.text = item.conteudo;

        if (badgeCategoria != null && txtBadgeCategoria != null)
        {
            txtBadgeCategoria.text = item.categoria.ToUpper();
            badgeCategoria.SetActive(!string.IsNullOrEmpty(item.categoria));
        }

        if (painelVazio != null) painelVazio.SetActive(false);
        intelDetailPanel.SetActive(true);
    }

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
    }

    [Header("Save System")]
    public IntelItem[] allPossibleIntel;

    public List<string> GetCollectedIntelNames()
    {
        List<string> names = new List<string>();
        foreach (var item in entradas)
        {
            names.Add(item.name);
        }
        return names;
    }

    public List<IntelItem> GetCollectedIntelItems() => new List<IntelItem>(entradas);

    public void RestoreIntelFromNames(List<string> names)
    {
        entradas.Clear();
        if (names == null) return;
        foreach (string n in names)
        {
            bool encontrado = false;
            foreach (var possible in allPossibleIntel)
            {
                if (possible.name == n)
                {
                    entradas.Add(possible);
                    encontrado = true;
                    break;
                }
            }
            // se isto disparar, o intel foi guardado corretamente no save mas não está na lista
            // "allPossibleIntel" deste componente — confirma no Inspector que o asset lá está.
            if (!encontrado)
                Debug.LogWarning($"[IntelInventory] Intel '{n}' estava no save mas não foi encontrado em allPossibleIntel.");
        }
        AtualizarTotalIntel();
    }

    //
    /// Esvazia a intel recolhida e repõe a UI do dossier, para que um "Novo Jogo" comece
    /// mesmo do zero (sem isto, a intel da partida anterior continuava toda lá).
    /// </summary>
    public void ResetForNewGame()
    {
        entradas.Clear();
        novosNaoVistos = 0;
        itemSelecionado = null;
        filtroAtual = "Todos";

        if (intelDetailPanel != null) intelDetailPanel.SetActive(false);
        if (painelVazio != null) painelVazio.SetActive(true);

        AtualizarBadgeNovoIntel();
        AtualizarTotalIntel();
        AtualizarLista();

        Debug.Log("[IntelInventory] Estado reiniciado para um novo jogo.");
    }
}