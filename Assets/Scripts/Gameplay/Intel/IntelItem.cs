using UnityEngine;

[CreateAssetMenu(menuName = "Intel/Novo Item")]
public class IntelItem : ScriptableObject
{
    public string titulo;
    public string categoria;
    public string localizacao;
    [TextArea] public string conteudo;
}