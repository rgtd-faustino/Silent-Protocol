using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailUI : MonoBehaviour
{
    // ------------------------------------------------------------------ //
    // Referência ao manager LOCAL deste PC                                  //
    // ------------------------------------------------------------------ //

    [Header("Manager deste PC")]
    [Tooltip("Arrasta aqui o PCEmailManager do mesmo PC. Se estiver no mesmo " +
             "GameObject pode deixar em branco — é auto-detectado.")]
    public PCEmailManager emailManager;

    // ------------------------------------------------------------------ //
    // Referências de UI                                                     //
    // ------------------------------------------------------------------ //

    [Header("Painel Raiz")]
    public GameObject emailAppPanel;

    [Header("Sidebar")]
    public Button btnInbox;
    public Button btnLixo;
    public GameObject badgeNaoLidos;
    public TextMeshProUGUI txtBadgeCount;

    [Header("Lista de Emails")]
    public Transform emailListContent;
    public GameObject emailEntryPrefab;

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
    // permite saber se algum email está aberto
    public static bool AlgumEmailAberto = false;
    private enum Vista { Inbox, Lixo }
    private Vista vistaAtual = Vista.Inbox;
    private EmailItem emailSelecionado = null;
    private List<GameObject> entradasAtivas = new List<GameObject>();
    private HashSet<EmailItem> intelJaGuardada = new HashSet<EmailItem>();

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Awake()
    {
        // Auto-detecta o manager se não foi atribuído no Inspector
        if (emailManager == null)
            emailManager = GetComponent<PCEmailManager>();

        if (emailManager == null)
            Debug.LogError($"[EmailUI] Nenhum PCEmailManager encontrado em '{gameObject.name}'. " +
                           "Arrasta-o para o campo 'emailManager' no Inspector.", this);
    }

    void Start()
    {
        if (emailManager == null) return;

        // Botões da sidebar
        btnInbox.onClick.AddListener(() => MudarVista(Vista.Inbox));
        btnLixo.onClick.AddListener(() => MudarVista(Vista.Lixo));

        // Botões do detalhe
        btnApagar.onClick.AddListener(ApagarEmailAtual);
        btnRestaurar.onClick.AddListener(RestaurarEmailAtual);
        btnGuardarIntel.onClick.AddListener(GuardarIntelAtual);

        // Reage a eventos do manager LOCAL deste PC
        emailManager.OnEmailRecebido += OnEmailRecebido;
        emailManager.OnEmailApagado += _ => AtualizarBadge();

        // Estado inicial
        emailDetailPanel.SetActive(false);
        AtualizarLista();
        AtualizarBadge();
    }

    void OnDestroy()
    {
        if (emailManager == null) return;
        emailManager.OnEmailRecebido -= OnEmailRecebido;
    }

    void Update()
    {
        // Remove este bloco se o teu sistema de PC já chama ToggleApp diretamente.
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
        AlgumEmailAberto = abrir;

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
        foreach (var go in entradasAtivas) Destroy(go);
        entradasAtivas.Clear();

        List<EmailItem> emails = vistaAtual == Vista.Inbox
            ? emailManager.GetInbox()
            : emailManager.GetLixo();

        foreach (var email in emails)
            CriarEntrada(email);
    }

    private void CriarEntrada(EmailItem email)
    {
        var go = Instantiate(emailEntryPrefab, emailListContent);
        entradasAtivas.Add(go);

        var labels = go.GetComponentsInChildren<TextMeshProUGUI>();
        if (labels.Length >= 1) labels[0].text = email.titulo;
        if (labels.Length >= 2) labels[1].text = email.remetenteNome;

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
        AtualizarLista();

        txtTitulo.text = email.titulo;
        txtRemetente.text = $"{email.remetenteNome}  <{email.remetente}>";
        txtDataHora.text = email.dataHora;
        txtCorpo.text = email.corpo;

        bool naInbox = vistaAtual == Vista.Inbox;
        btnApagar.gameObject.SetActive(naInbox);
        btnRestaurar.gameObject.SetActive(!naInbox);
        btnGuardarIntel.gameObject.SetActive(email.temIntel && email.intelAssociado != null);
        btnGuardarIntel.interactable = !intelJaGuardada.Contains(email);

        var label = btnGuardarIntel.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = intelJaGuardada.Contains(email) ? "Intel Guardada ✓" : "▼ GUARDAR INTEL";

        emailDetailPanel.SetActive(true);
    }

    // ------------------------------------------------------------------ //
    // Ações                                                                 //
    // ------------------------------------------------------------------ //

    private void ApagarEmailAtual()
    {
        if (emailSelecionado == null) return;
        emailManager.ApagarEmail(emailSelecionado);
        emailDetailPanel.SetActive(false);
        emailSelecionado = null;
        AtualizarLista();
    }

    private void RestaurarEmailAtual()
    {
        if (emailSelecionado == null) return;
        emailManager.RestaurarEmail(emailSelecionado);
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
        int naoLidos = emailManager.GetNaoLidos();
        if (badgeNaoLidos != null)
            badgeNaoLidos.SetActive(naoLidos > 0);
        if (txtBadgeCount != null)
            txtBadgeCount.text = naoLidos.ToString();
    }

    private void OnEmailRecebido(EmailItem email)
    {
        AtualizarBadge();
        if (emailAppPanel.activeSelf && vistaAtual == Vista.Inbox)
            AtualizarLista();
    }
}