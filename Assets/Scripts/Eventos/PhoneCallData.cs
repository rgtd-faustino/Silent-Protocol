using UnityEngine;

/// <summary>
/// Um canal de chamada telefónica.
/// Cria um asset por canal (ex: "EXT-04", "INT-07", "EXT-11").
/// Menu: Assets → Create → Phone → Novo Canal de Chamada
/// </summary>
[CreateAssetMenu(menuName = "Phone/Novo Canal de Chamada")]
public class PhoneCallData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("Label curto que aparece no botão de canal (ex: EXT-04).")]
    public string channelLabel = "EXT-00";

    [Header("Falas do canal (por ordem)")]
    [Tooltip("Cada entrada é uma frase que aparece em tempo real no transcript.")]
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Linhas com keyword capturável")]
    [Tooltip("Índices (base 0) das linhas de 'lines' que têm uma keyword. " +
             "O jogador prime SPACE enquanto a linha está visível para capturar.")]
    public int[] keywordLineIndices;

    [Header("Intel por keyword (mesmo índice que keywordLineIndices)")]
    [Tooltip("IntelItem entregue ao capturar cada keyword. " +
             "Pode ficar null — nesse caso só é contabilizado o score.")]
    public IntelItem[] intelRewards;

    [Header("Ritmo")]
    [Tooltip("Minutos de jogo entre linhas consecutivas.")]
    public float lineDelayGameMinutes = 1.5f;
}
