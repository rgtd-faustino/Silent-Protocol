using UnityEngine;
using static NPCScript;

[CreateAssetMenu(fileName = "NewNPCDialogueData", menuName = "Dialogue/NPC Dialogue Data")]
public class NPCDialogueData : ScriptableObject
{

    [Header("Identificacao")]
    public NPCType npcType;
    
    [Header("Suspeita")]
    [TextArea(2, 4)] public string suspicionGreeting = "O que estavas a fazer aqui?";
    [Range(0f, 1f)] public float suspicionThreshold = 0.33f;
    
    public string displayName = "NPC";

    [TextArea(2, 4)] public string greetingText = "Ola. Em que posso ajudar?";

    [Header("Topicos disponiveis")]
    // Agregamos o potencial todo aqui mas confiamos os calculos pesados de decisao nas flags logicas que habitam no DialogueManager.
    public DialogueTopic[] topics;

    [Header("Confronto (suspeita alta)")]
    public DialogueTopic confrontationTopic;
}