using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gere a lgica de email de UM PC especfico.
/// Entrega os emails quando a hora do jogo (GameClock) atinge o spawnHour do asset,
/// exactamente como o WireShark faz com os ScheduledPackets.
/// </summary>
public class PCEmailManager : MonoBehaviour
{
    [Header("Emails deste PC  (ordem no importa  entregues por hora do jogo)")]
    [SerializeField] private List<EmailItem> emailsDestePc = new List<EmailItem>();

    // ------------------------------------------------------------------ //
    // Listas runtime                                                        //
    // ------------------------------------------------------------------ //
    private List<EmailItem> inbox = new List<EmailItem>();
    private List<EmailItem> lixo = new List<EmailItem>();

    // emails ainda  espera de ser entregues
    private List<EmailItem> pendentes = new List<EmailItem>();

    // ------------------------------------------------------------------ //
    // Eventos (a EmailUI subscreve-os)                                      //
    // ------------------------------------------------------------------ //
    public event System.Action<EmailItem> OnEmailRecebido;
    public event System.Action<EmailItem> OnEmailApagado;

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Start()
    {
        foreach (var email in emailsDestePc)
        {
            if (email == null) continue;
            email.ResetarEstadoRuntime();
            pendentes.Add(email);
        }
    }

    void Update()
    {
        if (pendentes.Count == 0) return;

        float horaAtual = TimeManager.Instance.GetCurrentTimeInHours();

        for (int i = pendentes.Count - 1; i >= 0; i--)
        {
            var email = pendentes[i];
            if (horaAtual >= email.spawnHour)
            {
                pendentes.RemoveAt(i);
                EntregarEmail(email);
            }
        }
    }

    // ------------------------------------------------------------------ //
    // Entrega interna                                                       //
    // ------------------------------------------------------------------ //

    private void EntregarEmail(EmailItem email)
    {
        if (email.entregue) return;
        email.entregue = true;
        email.lido = false;
        email.apagado = email.irParaLixoDirectamente;

        if (email.irParaLixoDirectamente)
        {
            lixo.Add(email);
        }
        else
        {
            inbox.Add(email);
            OnEmailRecebido?.Invoke(email);
        }
    }

    // ------------------------------------------------------------------ //
    // API pblica                                                           //
    // ------------------------------------------------------------------ //

    /// <summary>Injeta um email em runtime (triggers, eventos de misso, etc.)</summary>
    public void ReceberEmail(EmailItem email)
    {
        if (inbox.Contains(email) || lixo.Contains(email)) return;
        email.lido = false;
        email.apagado = false;
        EntregarEmail(email);
    }

    /// <summary>Move o email para o Lixo (no apaga definitivamente).</summary>
    public void ApagarEmail(EmailItem email)
    {
        if (!inbox.Contains(email)) return;
        inbox.Remove(email);
        email.apagado = true;
        lixo.Add(email);
        OnEmailApagado?.Invoke(email);
    }

    /// <summary>Remove definitivamente do Lixo.</summary>
    public void ApagarDefinitivamente(EmailItem email)
    {
        lixo.Remove(email);
    }

    /// <summary>Restaura um email do Lixo para a Inbox.</summary>
    public void RestaurarEmail(EmailItem email)
    {
        if (!lixo.Contains(email)) return;
        lixo.Remove(email);
        email.apagado = false;
        inbox.Add(email);
    }

    public List<EmailItem> GetInbox() => new List<EmailItem>(inbox);
    public List<EmailItem> GetLixo() => new List<EmailItem>(lixo);

    public int GetNaoLidos()
    {
        int count = 0;
        foreach (var e in inbox)
            if (!e.lido) count++;
        return count;
    }
}