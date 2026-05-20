using UnityEngine;
using static NPCScript;

// ScriptableObject por tipo de NPC (ou por NPC individual no futuro)
// cria via Assets > Create > Dialogue > NPC Dialogue Data
[CreateAssetMenu(fileName = "NewNPCDialogueData", menuName = "Dialogue/NPC Dialogue Data")]

public class NPCDialogueData : ScriptableObject
{

    [Header("Identificao")]
    public NPCType npcType;
    [Header("Suspeita")]
    [TextArea(2, 4)] public string suspicionGreeting = "O que estavas a fazer aqui?";
    [Range(0f, 1f)] public float suspicionThreshold = 0.33f;
    // nome do NPC que aparece no UI de dilogo, ex: "Rececionista", "Guarda Miguel"
    public string displayName = "NPC";

    // frase inicial do NPC quando o jogador interage (antes de escolher tpico)
    [TextArea(2, 4)] public string greetingText = "Ol. Em que posso ajudar?";

    [Header("Tpicos disponveis")]
    // todos os tpicos possveis para este NPC  o DialogueManager filtra os que aparecem
    // consoante as condies (suspeita, charisma mnimo, etc.)
    public DialogueTopic[] topics;

    // tpico especial de confronto  aparece automaticamente quando suspeita >= threshold
    // se null, usa um dos topics com requiresHighSuspicion = true
    [Header("Confronto (suspeita alta)")]
    public DialogueTopic confrontationTopic;
}