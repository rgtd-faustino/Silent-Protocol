using UnityEngine;

[CreateAssetMenu(menuName = "Email/Novo Email")]
public class EmailItem : ScriptableObject
{
    [Header("Identificação")]
    // usamos esta string no GameEvent e no EmailManager para apagar emails críticos quando o tempo acaba
    public string emailID;

    [Header("Metadados")]
    public string titulo;
    public string remetenteNome;
    public string remetente;
    public string dataHora;
    [TextArea(3, 8)] public string corpo;

    [Header("Entrega")]
    public int diaParaAparecer = 1;
    // float decimal para facilitar contas de horas, o EmailManager bate de frente com o TimeManager.GetCurrentTimeInHours()
    public float spawnHour;
    public bool irParaLixoDirectamente;

    [Header("Intel Associada")]
    public bool temIntel;
    public IntelItem intelAssociado;

    [Header("Email Crítico")]
    [Tooltip("Mostra aviso UI")]
    public bool isCritical;

    [Tooltip("Corpo fica oculto")]
    public bool isEncrypted;

    [Tooltip("Minutos até expirar")]
    public float autoDeleteGameMinutes;

    // array com os IDs dos IntelItems que o jogador tem de colecionar. como estão em várias partes do mapa, forçamos exploração
    [Tooltip("IDs precisos")]
    public string[] requiredKeyFragmentIDs;

    [HideInInspector] public bool entregue;
    [HideInInspector] public bool lido;
    [HideInInspector] public bool apagado;
    [HideInInspector] public bool desencriptado;

    // como os ScriptableObjects mantêm valores em memória depois de sairmos do play mode, temos de dar reset antes de cada run
    public void ResetarEstadoRuntime()
    {
        entregue      = false;
        lido          = false;
        apagado       = false;
        desencriptado = false;
    }
}
