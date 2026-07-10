using UnityEngine;

[CreateAssetMenu(menuName = "Phone/Novo Canal de Chamada")]
public class PhoneCallData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Label")]
    public string channelLabel = "EXT-00";

    [Header("Falas do canal (por ordem)")]
    [Tooltip("Frases")]
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Linhas com keyword capturável")]
    [Tooltip("Indices keyword")]
    public int[] keywordLineIndices;

    [Header("Intel por keyword (mesmo índice que keywordLineIndices)")]
    [Tooltip("Rewards intel")]
    public IntelItem[] intelRewards;

    [Header("Ritmo")]
    [Tooltip("Espera")]
    public float lineDelayGameMinutes = 1.5f;
}
