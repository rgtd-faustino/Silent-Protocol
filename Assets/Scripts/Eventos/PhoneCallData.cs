using UnityEngine;

[CreateAssetMenu(menuName = "Phone/Novo Canal de Chamada")]
public class PhoneCallData : ScriptableObject
{
    [Header("Identificação")]
    public string channelLabel = "EXT-00"; // canal onde vai decorrer a chamada

    [Header("Falas do canal (por ordem)")]
    [TextArea(2, 5)]
    public string[] lines; // linhas de diálogo que a conversa vai ter

    [Header("Linhas com keyword capturável")]
    public int[] keywordLineIndices; // se tem alguma keyword importante que possa ser capturada

    [Header("Intel por keyword (mesmo índice que keywordLineIndices)")]
    public IntelItem[] intelRewards; // intel que o jogador poderá obter se captar a linha de diálogo que continha a keyword

    [Header("Ritmo")]
    public float lineDelayGameMinutes = 1.5f; // tempo de espera por linha de diálogo
}
