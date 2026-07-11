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
    public float spawnHour; // float decimal para facilitar contas de horas, o EmailManager bate de frente com o TimeManager.GetCurrentTimeInHours()
    public bool irParaLixoDirectamente;

    [Header("Intel Associada")]
    public bool temIntel;
    public IntelItem intelAssociado;

    public bool isCritical; // para ver se é necessário mostrar um aviso na UI
    public bool isEncrypted; // caso ativo, o corpo fica oculto
    public float autoDeleteGameMinutes; // minutos até expirar

    // array com os IDs dos IntelItems que o jogador tem de colecionar, como estão em várias partes do mapa forçamos a exploração
    public string[] requiredKeyFragmentIDs;

    [HideInInspector] public bool entregue;
    [HideInInspector] public bool lido;
    [HideInInspector] public bool apagado;
    [HideInInspector] public bool desencriptado;

    // ao longo do jogo algumas variáveis vão mudar por exemplo se o email foi lido e quando saímos do play mode as variáveis ficam iguais
    // então corremos este método em cada email para dar reset
    public void ResetarEstadoRuntime()
    {
        entregue = false;
        lido = false;
        apagado = false;
        desencriptado = false;
    }
}
