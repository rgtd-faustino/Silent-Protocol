using UnityEngine;

[CreateAssetMenu(menuName = "Email/Novo Email")]
public class EmailItem : ScriptableObject
{
    [Header("Metadados")]
    public string titulo;
    public string remetente;       // ex: "admin@corporatech.net"
    public string remetenteNome;   // ex: "Admin Corporatech"
    public string dataHora;        // ex: "2047-03-12  09:41"

    [Header("Contedo")]
    [TextArea(5, 20)]
    public string corpo;

    [Header("Intel")]
    public bool temIntel = false;
    [Tooltip("Se temIntel = true, este IntelItem ser guardado no inventrio ao clicar 'Guardar Intel'")]
    public IntelItem intelAssociado;

    [Header("Entrega")]
    [Tooltip("Hora do jogo em que este email chega  app.")]
    [Range(0f, 23.99f)]
    public float spawnHour = 9f;

    [Tooltip("Se true, o email cai directamente no Lixo em vez da Inbox.")]
    public bool irParaLixoDirectamente = false;

    // ------------------------------------------------------------------ //
    // Estado em runtime (no serializado  reinicia a cada sesso)         //
    // ------------------------------------------------------------------ //
    [HideInInspector] public bool lido = false;
    [HideInInspector] public bool apagado = false;
    [HideInInspector] public bool entregue = false;

    public void ResetarEstadoRuntime()
    {
        lido = false;
        apagado = false;
        entregue = false;
    }
}