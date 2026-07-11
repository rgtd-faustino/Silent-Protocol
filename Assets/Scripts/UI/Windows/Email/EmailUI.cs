using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailUI : MonoBehaviour
{

    [Header("Manager deste PC")]
    [Tooltip("Arrasta aqui o PCEmailManager do mesmo PC. Se estiver no mesmo " +
             "GameObject pode deixar em branco — é auto-detectado.")]
    public PCEmailManager emailManager;


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
    public Button btnEnviarRelatorio;

    [Header("Email Crítico")]
    public GameObject criticalEmailBanner; // banner vermelho no topo
    public TextMeshProUGUI txtAutoDeleteCountdown; // "Auto-delete em 4m 30s"
    public GameObject encryptedOverlay; // cobre o corpo do email
    public TextMeshProUGUI txtEncryptedHint; // "Precisas de 2 fragmentos"
    public Button btnDecrypt;
    public Button btnForward; // reencaminhar -> final Denúncia
    public Button btnDestroyEmail; // destruir -> final Lealdade

    // permite saber se algum email está aberto
    public static bool AlgumEmailAberto = false;
    private enum Vista { Inbox, Lixo }
    private Vista vistaAtual = Vista.Inbox;
    private EmailItem emailSelecionado = null;
    private List<GameObject> entradasAtivas = new List<GameObject>();
    private HashSet<EmailItem> intelJaGuardada = new HashSet<EmailItem>();


    void Awake()
    {
        // auto-detecta o manager se não foi atribuído no Inspector
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

        // "Enviar Relatório" só existe no PC do jogador —> este script é reutilizado por todos os PCs,
        // por isso só regista o listener se o botão estiver mesmo atribuído no Inspector
        if (btnEnviarRelatorio != null) btnEnviarRelatorio.onClick.AddListener(EnviarRelatorioAtual);

        // Botões email crítico
        if (btnDecrypt != null) btnDecrypt.onClick.AddListener(TentarDesencriptar);
        if (btnForward != null) btnForward.onClick.AddListener(ReencaminharEmailAtual);
        if (btnDestroyEmail != null) btnDestroyEmail.onClick.AddListener(DestruirEmailAtual);
        GameEvent.OnCriticalEmailExpired += OnCriticalEmailExpired;

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
        if (emailManager == null) 
            return;

        emailManager.OnEmailRecebido -= OnEmailRecebido;
        GameEvent.OnCriticalEmailExpired -= OnCriticalEmailExpired;
    }

    void Update()
    {
        if (emailAppPanel != null && emailAppPanel.activeSelf)
        {
            if (TutorialManager.Instance.IsCurrentStepGate("tut_email"))
            {
                TutorialManager.Instance.CompleteCurrentStep();
            }
        }

        // Remove este bloco se o teu sistema de PC já chama ToggleApp diretamente.
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleApp();

        // countdown de auto-delete do email crítico aberto
        if (emailSelecionado != null && emailSelecionado.isCritical && txtAutoDeleteCountdown != null)
        {
            float restante = emailManager.GetAutoDeleteTimeRemaining(emailSelecionado);
            if (restante > 0f)
            {
                int m = Mathf.FloorToInt(restante / 60f);
                int s = Mathf.FloorToInt(restante % 60f);
                txtAutoDeleteCountdown.text = $"AUTO-DELETE EM {m}m {s:00}s";
            }
            else if (restante < 0f)
            {
                txtAutoDeleteCountdown.text = "";
            }
        }
    }


    public void ToggleApp()
    {
        bool abrir = !emailAppPanel.activeSelf;
        emailAppPanel.SetActive(abrir);
        AlgumEmailAberto = abrir;

        if (abrir)
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_email"))
            {
                TutorialManager.Instance.CompleteCurrentStep();
            }

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

        List<EmailItem> emails = vistaAtual == Vista.Inbox ? emailManager.GetInbox() : emailManager.GetLixo();

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

    private void AbrirEmail(EmailItem email)
    {
        emailSelecionado = email;
        email.lido = true;
        AtualizarBadge();
        AtualizarLista();

        txtTitulo.text = email.titulo;
        txtRemetente.text = $"{email.remetenteNome}  <{email.remetente}>";
        txtDataHora.text = email.dataHora;
        txtCorpo.text = (email.isEncrypted && !email.desencriptado) ? "<color=#888888>[ENCRIPTADO — usa os teus fragmentos de chave para desencriptar]</color>" : email.corpo;

        bool naInbox = vistaAtual == Vista.Inbox;
        // emails críticos não podem ser apagados normalmente — só destruídos pelo botão próprio
        btnApagar.gameObject.SetActive(naInbox && !email.isCritical);
        btnRestaurar.gameObject.SetActive(!naInbox);
        btnGuardarIntel.gameObject.SetActive(email.temIntel && email.intelAssociado != null);
        btnGuardarIntel.interactable = !intelJaGuardada.Contains(email);

        var label = btnGuardarIntel.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = intelJaGuardada.Contains(email) ? "Intel Guardada" : "GUARDAR INTEL";

        // reset UI crítica
        if (criticalEmailBanner != null) criticalEmailBanner.SetActive(false);
        if (encryptedOverlay != null) encryptedOverlay.SetActive(false);
        if (btnDecrypt != null) btnDecrypt.gameObject.SetActive(false);
        if (btnForward != null) btnForward.gameObject.SetActive(false);
        if (btnDestroyEmail != null) btnDestroyEmail.gameObject.SetActive(false);
        if (txtAutoDeleteCountdown != null) txtAutoDeleteCountdown.text = "";

        if (email.isCritical) ConfigurarEmailCritico(email);

        emailDetailPanel.SetActive(true);
    }

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
        if (label != null) label.text = "Intel Guardada";
    }

    private void EnviarRelatorioAtual()
    {
        Debug.Log("[EmailUI] Relatório enviado — a acionar o final do jogo.");
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerReportEnding();
    }

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

    private void ConfigurarEmailCritico(EmailItem email)
    {
        if (criticalEmailBanner != null) criticalEmailBanner.SetActive(true);

        bool encriptado = email.isEncrypted && !email.desencriptado;
        if (encryptedOverlay != null) encryptedOverlay.SetActive(encriptado);
        if (btnDecrypt != null) btnDecrypt.gameObject.SetActive(encriptado);
        if (btnForward != null) btnForward.gameObject.SetActive(!encriptado);
        if (btnDestroyEmail != null) btnDestroyEmail.gameObject.SetActive(true);

        if (encriptado && txtEncryptedHint != null)
        {
            int precisa = email.requiredKeyFragmentIDs != null ? email.requiredKeyFragmentIDs.Length : 0;
            int tem = ContarFragmentosDisponiveis(email);
            txtEncryptedHint.text = $"Encriptado — precisas de {precisa} fragmento(s) de chave.\nTens: {tem}/{precisa}";
        }
    }

    private int ContarFragmentosDisponiveis(EmailItem email)
    {
        if (email.requiredKeyFragmentIDs == null) return 0;
        int count = 0;
        List<IntelItem> colectados = IntelInventory.Instance.GetCollectedIntelItems();
        foreach (string reqID in email.requiredKeyFragmentIDs)
            foreach (IntelItem intel in colectados)
                if (intel.isKeyFragment && intel.keyFragmentID == reqID) { count++; break; }
        return count;
    }

    private void TentarDesencriptar()
    {
        if (emailSelecionado == null) return;
        int precisa = emailSelecionado.requiredKeyFragmentIDs != null ? emailSelecionado.requiredKeyFragmentIDs.Length : 0;
        int tem = ContarFragmentosDisponiveis(emailSelecionado);

        if (tem >= precisa)
        {
            emailSelecionado.desencriptado = true;
            txtCorpo.text = emailSelecionado.corpo;
            if (encryptedOverlay != null) encryptedOverlay.SetActive(false);
            if (btnDecrypt != null) btnDecrypt.gameObject.SetActive(false);
            if (btnForward != null) btnForward.gameObject.SetActive(true);
            if (txtEncryptedHint != null) txtEncryptedHint.text = "";
            Debug.Log("[EmailUI] Email desencriptado com sucesso.");
        }
        else
        {
            if (txtEncryptedHint != null)
                txtEncryptedHint.text = $"<color=#FF4444>Fragmentos insuficientes! ({tem}/{precisa})</color>";
        }
    }

    private void ReencaminharEmailAtual()
    {
        if (emailSelecionado == null) return;
        GameManager.Instance.RegisterEndingContribution(0, emailSelecionado.emailID);
        emailManager.ApagarEmail(emailSelecionado);
        emailDetailPanel.SetActive(false);
        emailSelecionado = null;
        AtualizarLista();
        Debug.Log("[EmailUI] Email reencaminhado — contribuição para final Denúncia.");
    }

    private void DestruirEmailAtual()
    {
        if (emailSelecionado == null) return;
        GameManager.Instance.RegisterEndingContribution(2, emailSelecionado.emailID);
        emailManager.ApagarDefinitivamente(emailSelecionado);
        emailDetailPanel.SetActive(false);
        emailSelecionado = null;
        AtualizarLista();
        Debug.Log("[EmailUI] Email destruído — contribuição para final Lealdade.");
    }

    private void OnCriticalEmailExpired(string emailID)
    {
        if (emailSelecionado != null && emailSelecionado.emailID == emailID)
        {
            emailDetailPanel.SetActive(false);
            emailSelecionado = null;
        }
        AtualizarLista();
        Debug.LogWarning($"[EmailUI] Email crítico '{emailID}' expirou e foi auto-deletado.");
    }
}