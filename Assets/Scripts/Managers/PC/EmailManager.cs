using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gere toda a lógica de email: inbox, lixo, e receção de emails em runtime.
/// Usa o padrão Singleton para ser acedido de qualquer script.
///
/// Como enviar um email por evento/script:
///   EmailManager.Instance.ReceberEmail(meuEmailItem);
///
/// Como pré-carregar emails no inspector:
///   Arrasta EmailItem assets para a lista 'emailsIniciais'.
/// </summary>
public class EmailManager : MonoBehaviour
{
    public static EmailManager Instance;

    [Header("Emails que já existem na inbox ao iniciar o jogo")]
    [SerializeField] private List<EmailItem> emailsIniciais = new List<EmailItem>();

    // listas em runtime
    private List<EmailItem> inbox = new List<EmailItem>();
    private List<EmailItem> lixo = new List<EmailItem>();

    // evento para que a UI reaja automaticamente quando chega um novo email
    public event System.Action<EmailItem> OnEmailRecebido;
    public event System.Action<EmailItem> OnEmailApagado;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // carrega os emails iniciais definidos no inspector
        foreach (var email in emailsIniciais)
            inbox.Add(email);
    }

    // ------------------------------------------------------------------ //
    // API pública                                                           //
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Recebe um email em runtime (chamado por triggers, scripts de missão, etc.)
    /// </summary>
    public void ReceberEmail(EmailItem email)
    {
        if (inbox.Contains(email) || lixo.Contains(email)) return;

        email.lido = false;
        email.apagado = false;
        inbox.Add(email);

        OnEmailRecebido?.Invoke(email);
    }

    /// <summary>
    /// Move o email para o Lixo (não apaga definitivamente).
    /// </summary>
    public void ApagarEmail(EmailItem email)
    {
        if (!inbox.Contains(email)) return;

        inbox.Remove(email);
        email.apagado = true;
        lixo.Add(email);

        OnEmailApagado?.Invoke(email);
    }

    /// <summary>
    /// Remove definitivamente do Lixo.
    /// </summary>
    public void ApagarDefinitivamente(EmailItem email)
    {
        lixo.Remove(email);
    }

    /// <summary>
    /// Restaura um email do Lixo para a Inbox.
    /// </summary>
    public void RestaurarEmail(EmailItem email)
    {
        if (!lixo.Contains(email)) return;

        lixo.Remove(email);
        email.apagado = false;
        inbox.Add(email);
    }

    public List<EmailItem> GetInbox() => new List<EmailItem>(inbox);
    public List<EmailItem> GetLixo() => new List<EmailItem>(lixo);

    /// <summary>
    /// Número de emails não lidos na inbox (útil para mostrar badge no HUD).
    /// </summary>
    public int GetNaoLidos()
    {
        int count = 0;
        foreach (var e in inbox)
            if (!e.lido) count++;
        return count;
    }
}