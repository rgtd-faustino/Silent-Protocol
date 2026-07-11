using UnityEngine;

[CreateAssetMenu(fileName = "NewTopic", menuName = "Dialogue/Topic")]
public class DialogueTopic : ScriptableObject
{
    [Header("Identificacao")]
    public string topicID;
    public string buttonLabel;

    [Header("Tipo de topico")]
    public TopicType topicType;

    // controla o fluxo de respostas visiveis baseado na avaliacao continua de risco no DialogueManager para so mostrar as opcoes certas na pior das situacoes.
    [Header("Condicao de aparecimento")]
    public bool requiresHighSuspicion = false;
    [Range(0f, 1f)] public float suspicionThreshold = 0.33f;

    public int requiredCharisma = 0;

    // Este vetor dita a consequencia da resposta baseada nos checks.
    [Header("Resultado")]
    public TopicOutcome[] outcomes;


    public enum TopicType
    {
        Question,
        Statement,
        Request,
        Confrontation
    }

    // Comparamos em cascata os pormenores estatisticos do boneco face ao que temos nos ficheiros para disparar a melhor recompensa de resposta possivel. O final da lista entra como um fallback bruto se tudo falhar.
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

        return outcomes[outcomes.Length - 1];
    }
}


[System.Serializable]
public class TopicOutcome
{
    [Header("Tem potencial de Intel?")]
    public bool temIntel = false;
    public IntelItem intelAssociado;
    public bool intelJaRecolhida = false;
    
    [Header("Check")]
    public StatCheck statCheck;

    // Fasquia base que as definicoes do utilizador teem de ultrapassar para esta ramificacao passar limpa no algoritmo principal.
    [Range(1, 10)] public int threshold = 5;
    
    // A sorte ampara os erros reduzindo as consequencias da penalizacao original. Parametro crucial para as buids de charme e sorte.
    [Range(0, 3)] public int luckModifier = 1;

    public bool suspicionAffects = false;
    
    // Incrementa brutalmente a dificuldade do check. O pessoal da empresa farta-se de falar se o ambiente andar pesado por causa de coisas estragadas nas secretarias.
    [Range(0, 5)] public int suspicionPenalty = 2;

    [Header("Resposta do NPC")]
    [TextArea(2, 5)] public string npcResponse;

    [Header("Consequencias")]
    public ConsequenceType consequence;
    public float consequenceAmount = 0f;
    public int unlockFloorIndex = -1;

    public enum StatCheck { Charisma, Luck, None }

    public enum ConsequenceType
    {
        None,
        DecreaseSuspicion,
        IncreaseSuspicion,
        UnlockFloor,
        GiveIntel
    }

    // Calcula os pormenores matematicos em runtime absorvendo toda a info estatistica e verificando se os debuffs quebram a fasquia definida no unity.
    public bool CheckPasses(int charisma, int luck, float suspicion)
    {
        if (statCheck == StatCheck.None)
            return true;

        int effectiveThreshold = threshold;

        effectiveThreshold -= Mathf.RoundToInt(luck * luckModifier / 10f);

        if (suspicionAffects && suspicion >= 0.66f)
            effectiveThreshold += suspicionPenalty;

        int stat = statCheck == StatCheck.Charisma ? charisma : luck;
        return stat >= effectiveThreshold;
    }
}