using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a UI da aplicação de email no computador do jogador.
///
/// Estrutura da UI esperada (ver guia abaixo):
///
///  EmailApp (Panel raiz)
///  ├── Sidebar
///  │   ├── BtnInbox          (Button)
///  │   └── BtnLixo           (Button)
///  ├── EmailListPanel
///  │   └── EmailListContent  (Transform – filho de ScrollRect)
///  ├── EmailDetailPanel
///  │   ├── TxtTitulo         (TextMeshProUGUI)
///  │   ├── TxtRemetente      (TextMeshProUGUI)
///  │   ├── TxtDataHora       (TextMeshProUGUI)
///  │   ├── TxtCorpo          (TextMeshProUGUI)
///  │   ├── BtnApagar         (Button) — visível só na Inbox
///  │   ├── BtnRestaurar      (Button) — visível só no Lixo
///  │   └── BtnGuardarIntel   (Button) — visível só se temIntel = true
///  └── EmailEntryPrefab      (prefab com Button + TxtTitulo + TxtRemetente + IconoNaoLido)
/// </summary>
public class EmailUI : MonoBehaviour
{
    public static EmailUI Instance;

    [Header("Painel Raiz")]
    public GameObject emailAppPanel;

    [Header("Sidebar")]
    public Button btnInbox;
    public Button btnLixo;
    // imagem/objeto de badge para não-lidos (opcional)
    public GameObject badgeNaoLidos;
    public TextMeshProUGUI txtBadgeCount;

    [Header("Lista de Emails")]
    public Transform emailListContent;       // Content do ScrollRect
    public GameObject emailEntryPrefab;      // prefab de cada linha da lista
    private HashSet<EmailItem> intelJaGuardada = new HashSet<EmailItem>();

    [Header("Painel de Detalhe")]
    public GameObject emailDetailPanel;
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtRemetente;
    public TextMeshProUGUI txtDataHora;
    public TextMeshProUGUI txtCorpo;
    public Button btnApagar;
    public Button btnRestaurar;
    public Button btnGuardarIntel;


    // ------------------------------------------------------------------ //
    // Estado interno                                                        //
    // ------------------------------------------------------------------ //

    private enum Vista { Inbox, Lixo }
    private Vista vistaAtual = Vista.Inbox;
    private EmailItem emailSelecionado = null;
    private List<GameObject> entradasAtivas = new List<GameObject>();

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
        // botões da sidebar
        btnInbox.onClick.AddListener(() => MudarVista(Vista.Inbox));
        btnLixo.onClick.AddListener(() => MudarVista(Vista.Lixo));

        // botões do detalhe
        btnApagar.onClick.AddListener(ApagarEmailAtual);
        btnRestaurar.onClick.AddListener(RestaurarEmailAtual);
        btnGuardarIntel.onClick.AddListener(GuardarIntelAtual);

        // reage a novos emails em tempo real
        EmailManager.Instance.OnEmailRecebido += OnEmailRecebido;
        EmailManager.Instance.OnEmailApagado += _ => AtualizarBadge();

        // estado inicial
        emailDetailPanel.SetActive(false);
        AtualizarLista();
        AtualizarBadge();
    }

    void OnDestroy()
    {
        if (EmailManager.Instance == null) return;
        EmailManager.Instance.OnEmailRecebido -= OnEmailRecebido;
    }

    void Update()
    {
        // abre/fecha a app de email com Tab (ou o teu sistema de PC pode chamar ToggleApp diretamente)
        // Remove este bloco se o PC já chama ToggleApp pelo seu próprio script.
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleApp();
    }

    // ------------------------------------------------------------------ //
    // API pública                                                           //
    // ------------------------------------------------------------------ //

    public void ToggleApp()
    {
        bool abrir = !emailAppPanel.activeSelf;
        emailAppPanel.SetActive(abrir);

        if (abrir)
        {
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

    // ------------------------------------------------------------------ //
    // Vista / Lista                                                         //
    // ------------------------------------------------------------------ //

    private void MudarVista(Vista vista)
    {
        vistaAtual = vista;
        emailDetailPanel.SetActive(false);
        emailSelecionado = null;
        AtualizarLista();
    }

    private void AtualizarLista()
    {
        // limpar entradas antigas
        foreach (var go in entradasAtivas)
            Destroy(go);
        entradasAtivas.Clear();

        List<EmailItem> emails = vistaAtual == Vista.Inbox
            ? EmailManager.Instance.GetInbox()
            : EmailManager.Instance.GetLixo();

        foreach (var email in emails)
            CriarEntrada(email);
    }

    private void CriarEntrada(EmailItem email)
    {
        var go = Instantiate(emailEntryPrefab, emailListContent);
        entradasAtivas.Add(go);

        // --- adapta os nomes abaixo à tua hierarquia de prefab ---
        var labels = go.GetComponentsInChildren<TextMeshProUGUI>();
        if (labels.Length >= 1) labels[0].text = email.titulo;
        if (labels.Length >= 2) labels[1].text = email.remetenteNome;

        // ícone de não-lido (objeto com nome "IconoNaoLido" no prefab, opcional)
        var icone = go.transform.Find("IconoNaoLido");
        if (icone != null) icone.gameObject.SetActive(!email.lido);

        go.GetComponent<Button>().onClick.AddListener(() => AbrirEmail(email));
    }

    // ------------------------------------------------------------------ //
    // Detalhe                                                               //
    // ------------------------------------------------------------------ //

    private void AbrirEmail(EmailItem email)
    {
        emailSelecionado = email;
        email.lido = true;
        AtualizarBadge();

        // atualizar ícone de não-lido na lista sem reconstruir tudo
        AtualizarLista();

        // preencher detalhe
        txtTitulo.text = email.titulo;
        txtRemetente.text = $"{email.remetenteNome}  <{email.remetente}>";
        txtDataHora.text = email.dataHora;
        txtCorpo.text = email.corpo;

        // mostrar botões corretos conforme a vista
        bool naInbox = vistaAtual == Vista.Inbox;
        btnApagar.gameObject.SetActive(naInbox);
        btnRestaurar.gameObject.SetActive(!naInbox);
        btnGuardarIntel.gameObject.SetActive(email.temIntel && email.intelAssociado != null);
        btnGuardarIntel.interactable = !intelJaGuardada.Contains(email);
        var label = btnGuardarIntel.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = intelJaGuardada.Contains(email) ? "Intel Guardada ✓" : "▼ GUARDAR INTEL";
        emailDetailPanel.SetActive(true);
    }

    // ------------------------------------------------------------------ //
    // Ações                                                                 //
    // ------------------------------------------------------------------ //

    private void ApagarEmailAtual()
    {
        if (emailSelecionado == null) return;
        EmailManager.Instance.ApagarEmail(emailSelecionado);
        emailDetailPanel.SetActive(false);
        emailSelecionado = null;
        AtualizarLista();
    }

    private void RestaurarEmailAtual()
    {
        if (emailSelecionado == null) return;
        EmailManager.Instance.RestaurarEmail(emailSelecionado);
        emailDetailPanel.SetActive(false);
        emailSelecionado = null;
        AtualizarLista();
    }

    private void GuardarIntelAtual()
    {
        if (emailSelecionado == null || !emailSelecionado.temIntel) return;
        if (intelJaGuardada.Contains(emailSelecionado)) return;

        IntelInventory.Instance.AdicionarIntel(emailSelecionado.intelAssociado);
        intelJaGuardada.Add(emailSelecionado);

        btnGuardarIntel.interactable = false;
        var label = btnGuardarIntel.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = "Intel Guardada ✓";
    }

    // ------------------------------------------------------------------ //
    // Badge de não-lidos                                                    //
    // ------------------------------------------------------------------ //

    private void AtualizarBadge()
    {
        int naoLidos = EmailManager.Instance.GetNaoLidos();
        if (badgeNaoLidos != null)
            badgeNaoLidos.SetActive(naoLidos > 0);
        if (txtBadgeCount != null)
            txtBadgeCount.text = naoLidos.ToString();
    }

    private void OnEmailRecebido(EmailItem email)
    {
        AtualizarBadge();
        // se a app estiver aberta e estivermos na inbox, atualiza a lista em direto
        if (emailAppPanel.activeSelf && vistaAtual == Vista.Inbox)
            AtualizarLista();
    }
}