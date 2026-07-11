using UnityEngine;

// scriptableObject de dados de um item de intel, cada instancia é um asset separado que o IntelInventory guarda em runtime após o jogador o apanhar
[CreateAssetMenu(menuName = "Intel/Novo Item")]
public class IntelItem : ScriptableObject
{
    public string titulo;
    public string categoria;
    public string localizacao;
    [TextArea] public string conteudo;

    [Header("Fragmento de Chave (emails encriptados)")]
    // se true, este item contribui para desencriptar emails no WireShark
    // o CryptoHelper verifica o keyFragmentID contra os fragmentos no inventário para montar a chave completa
    public bool isKeyFragment;
    public string keyFragmentID;

    [Header("Contribuicao para o Final")]
    // o GameManager soma estas percentagens para decidir qual o final a disparar no ultimo dia
    [Tooltip("Quanto esta intel contribui para a percentagem total no final do jogo.")]
    [Range(0f, 100f)]
    public float percentagemContribuicao = 0f;
}
