using UnityEngine;

[CreateAssetMenu(menuName = "Email/Novo Email")]
public class EmailItem : ScriptableObject
{
    [Header("Identificação")]
    public string emailID; // ID único — usado pelo sistema de auto-delete e endings

    [Header("Metadados")]
    public string titulo;
    public string remetenteNome;
    public string remetente;   // endereço (ex: ceo@corp.com)
    public string dataHora;
    [TextArea(3, 8)] public string corpo;

    [Header("Entrega")]
    public int diaParaAparecer = 1;
    public float spawnHour;            // hora do jogo em que aparece na inbox
    public bool irParaLixoDirectamente;

    [Header("Intel Associada")]
    public bool temIntel;
    public IntelItem intelAssociado;

    [Header("Email Crítico")]
    [Tooltip("Activa o banner vermelho e os botões especiais na UI.")]
    public bool isCritical;

    [Tooltip("Se true, o corpo fica oculto até o jogador desencriptar.")]
    public bool isEncrypted;

    [Tooltip("Minutos de jogo até auto-delete. 0 = sem limite.")]
    public float autoDeleteGameMinutes;

    [Tooltip("IDs dos IntelItems com isKeyFragment=true necessários para desencriptar.")]
    public string[] requiredKeyFragmentIDs;

    // ------------------------------------------------------------------ //
    // Estado runtime — não configurar no Inspector                         //
    // ------------------------------------------------------------------ //
    [HideInInspector] public bool entregue;
    [HideInInspector] public bool lido;
    [HideInInspector] public bool apagado;
    [HideInInspector] public bool desencriptado;

    public void ResetarEstadoRuntime()
    {
        entregue      = false;
        lido          = false;
        apagado       = false;
        desencriptado = false;
    }
}
