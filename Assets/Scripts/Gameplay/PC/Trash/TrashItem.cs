using UnityEngine;

[CreateAssetMenu(menuName = "TrashBin/Novo Item")]
public class TrashItem : ScriptableObject
{
    [Header("Conteúdo")]
    public string titulo; // nome do ficheiro no lixo

    [TextArea(5, 20)]
    public string corpo; // conteúdo do ficheiro

    [Header("Intel")]
    public bool temIntel = false; // se tiver intel associamos o intel item que é depois transferido para o inventário
    public IntelItem intelAssociado;

    [Header("Entrega")]
    [Range(0f, 23.99f)]
    public float spawnHour = 9f; // hora do jogo em que o item aparece no caixote do lixo do computador

    // controla se o item já foi gerado na interface na sessão atual
    [HideInInspector] public bool entregue = false;

    // usado pelos managers para limpar o estado quando o jogador reinicia o dia (porque a memória é mantida nos scriptable objects)
    public void ResetarEstadoRuntime()
    {
        entregue = false;
    }
}