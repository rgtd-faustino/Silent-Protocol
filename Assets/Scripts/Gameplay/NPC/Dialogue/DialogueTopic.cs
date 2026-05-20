using UnityEngine;

// Um tpico de conversa  cria via Assets > Create > Dialogue > Topic
[CreateAssetMenu(fileName = "NewTopic", menuName = "Dialogue/Topic")]
public class DialogueTopic : ScriptableObject
{

    [Header("Identificao")]
    public string topicID;          //
                                    // nico, ex: "ask_decoding", "invite_coffee"
    public string buttonLabel;       // texto que aparece no boto, ex: "Como se faz a descodificao?"

    [Header("Tipo de tpico")]
    public TopicType topicType;

    // se true, este tpico s aparece quando suspeita >= suspicionThreshold
    [Header("Condio de aparecimento")]
    public bool requiresHighSuspicion = false;
    [Range(0f, 1f)] public float suspicionThreshold = 0.33f;

 

    // mnimo de carisma para este tpico aparecer na lista (0 = sempre aparece)
    public int requiredCharisma = 0;

    [Header("Resultado")]
    public TopicOutcome[] outcomes; // avaliados por ordem  o primeiro cujo check passa  usado


    public enum TopicType
    {
        Question,       // o jogador quer saber algo
        Statement,      // o jogador afirma/partilha algo
        Request,        // o jogador pede algo ao NPC
        Confrontation   // aparece quando suspeita alta  mentira ou verdade
    }

    // Avalia os outcomes por ordem e devolve o primeiro que passa o check de stats
    // Se nenhum passar devolve o ltimo (deve ser sempre o fallback)
    public TopicOutcome Evaluate()
    {
        int charisma = PlayerStats.Instance.GetCarisma();
        int luck = PlayerStats.Instance.GetSorte();
        float suspicion = SuspicionManager.Instance.GetSuspicionRatio();

        for (int i = 0; i < outcomes.Length; i++)
        {
            if (outcomes[i].CheckPasses(charisma, luck, suspicion))
                return outcomes[i];
        }

        return outcomes[outcomes.Length - 1]; // fallback
    }
}


[System.Serializable]
public class TopicOutcome
{
    //se true, ento um boto ir aparecer para o jogador guardar a intel
    [Header("Tem potncial de Intel?")]
    public bool temIntel = false;
    public IntelItem intelAssociado;
    public bool intelJaRecolhida = false;
    [Header("Check")]
    // qual stat  avaliada neste outcome
    public StatCheck statCheck;

    // o threshold base  se a stat >= threshold este outcome  escolhido.
    // luck modifica o threshold em luckModifier (positivo = mais fcil com boa sorte)
    [Range(1, 10)] public int threshold = 5;
    [Range(0, 3)] public int luckModifier = 1; // quanto a sorte pode baixar o threshold

    // se true, suspeita alta aumenta o threshold em suspicionPenalty pontos
    public bool suspicionAffects = false;
    [Range(0, 5)] public int suspicionPenalty = 2; // adicionado ao threshold se suspeita >= 0.66

    [Header("Resposta do NPC")]
    [TextArea(2, 5)] public string npcResponse;

    [Header("Consequncias")]
    public ConsequenceType consequence;
    public float consequenceAmount = 0f; // usado por ChangeSuspicion e UnlockFloor
    public int unlockFloorIndex = -1;    // usado por UnlockFloor

    public enum StatCheck { Charisma, Luck, None }

    public enum ConsequenceType
    {
        None,
        DecreaseSuspicion,
        IncreaseSuspicion,
        UnlockFloor,
        GiveIntel          // para quando o NPC d uma pista  implementar com IntelInventory
    }

    // devolve true se este outcome deve ser usado
    public bool CheckPasses(int charisma, int luck, float suspicion)
    {
        if (statCheck == StatCheck.None)
            return true;

        int effectiveThreshold = threshold;

        // sorte reduz o threshold (mais fcil passar)
        effectiveThreshold -= Mathf.RoundToInt(luck * luckModifier / 10f);

        // suspeita alta aumenta o threshold (mais difcil passar)
        if (suspicionAffects && suspicion >= 0.66f)
            effectiveThreshold += suspicionPenalty;

        int stat = statCheck == StatCheck.Charisma ? charisma : luck;
        return stat >= effectiveThreshold;
    }
}