using UnityEngine;

[CreateAssetMenu(menuName = "TrashBin/Novo Item")]
public class TrashItem : ScriptableObject
{
    [Header("Conteúdo")]
    public string titulo;

    [TextArea(5, 20)]
    public string corpo;

    [Header("Intel")]
    public bool temIntel = false;
    [Tooltip("Define se ao clicar para guardar a informação, o IntelItem é transferido para o inventário.")]
    public IntelItem intelAssociado;

    [Header("Entrega")]
    [Tooltip("Hora do jogo em que este item aparece no caixote do lixo.")]
    [Range(0f, 23.99f)]
    public float spawnHour = 9f;

    // Controla se o item já foi gerado na interface na sessão atual
    // Oculto no inspetor porque é gerido dinamicamente e não queremos alterar o estado no ScriptableObject permanentemente
    [HideInInspector] public bool entregue = false;

    // Usado pelos managers para limpar o estado quando o jogador reinicia o dia
    public void ResetarEstadoRuntime()
    {
        entregue = false;
    }
}