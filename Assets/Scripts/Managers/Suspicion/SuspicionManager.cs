using System.Collections.Generic;
using UnityEngine;

// #my_code - Sistema duplo de suspeita: company awareness + suspeita individual por fonte
public class SuspicionManager : MonoBehaviour
{

    public static SuspicionManager Instance;

    AudioSource heartbeatSource;

    [SerializeField] private float maxSuspicion = 1f;
    private float currentSuspicion = 0f;

    // velocidade base a que a suspeita sobe por segundo quando há uma fonte ativa (ex: NPC a ver o jogador) multiplicada pelo "level" da fonte (1, 1.5 ou 2) para fontes mais graves
    [SerializeField] private float baseIncreaseSpeed = 0.1f; 
    [SerializeField] private float decayDelay = 10f; // segundos sem descer mais depois da fonte parar antes da suspeita começaar a baixar
    [SerializeField] private float decaySpeed = 0.03f; // velocidade a que a suspeita baixa por segundo durante o decay

    
    private Dictionary<int, float> activeSources = new Dictionary<int, float>();
    private float timeSinceLastIncrease = 0f; // contador para o decayDelay
    private bool isDecaying = false; // flag que ativa o decay após o delay expirar

    // estado atual (None/Attention/Investigation/Expulsion) guardado para detetar mudanças e disparar o evento apenas quando muda
    private SuspicionState currentState = SuspicionState.None;


    // None -> comportamento normal
    // Attention -> NPCs observam mais (>33% da barra)
    // Investigation -> guardas aumentam patrulhas (>66%)
    // Expulsion -> Game Over (100%)
    public enum SuspicionState
    {
        None,
        Attention,
        Investigation,
        Expulsion
    }

    public enum SuspicionSource
    {
        NPCSight, // NPC vê o jogador numa zona suspeita
        RestrictedArea, // jogador está numa zona restrita
        Camera, // acesso excessivo a câmaras
        Noise, // barulho à noite (guarda ouviu o jogador)
        TerminalAccess, // acesso a terminais fora do posto de trabalho
        DocumentMisfiled, // documento arquivado no departamento errado
        Hacking, // hackear as camaras para lhes aceder 
        CardCodeDenied // tentar aceder com um cartão ou código e errar
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        heartbeatSource = SoundManager.Instance.heartbeatSource;
    }

    void Update() {
        float totalRate = 0f;
        // apanhamos a suspeita total de todas as fontes
        foreach (float value in activeSources.Values)
            totalRate += value;

        // há uma fonte ativa -> a suspeita sobe
        if (totalRate > 0f) {
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + totalRate * Time.deltaTime);
            // reset do contador de decay -> enquanto há fonte ativa o timer não avança
            timeSinceLastIncrease = 0f;
            isDecaying = false;

        } else {
            // sem fonte ativa -> conta o tempo antes do decay
            timeSinceLastIncrease += Time.deltaTime;

            if (timeSinceLastIncrease >= decayDelay)
                isDecaying = true;

            // baixa a suspeita gradualmente após o delay
            if (isDecaying && currentSuspicion > 0) {
                currentSuspicion = Mathf.Max(0f, currentSuspicion - decaySpeed * Time.deltaTime);

                if (currentSuspicion <= 0)
                    isDecaying = false;
            }
        }

        CheckStateChange();
        UpdateHeartbeat();
    }


    // level vai de 1 a 3 e representa a gravidade da situação atual do jogador
    public void IncreaseSuspicion(float level, int sourceId, SuspicionSource source = SuspicionSource.NPCSight)
    {
        if (level < 1 || level > 3)
            return; // valores fora do intervalo são ignorados

        // atributo Sorte reduz a taxa de ganho de suspeita contínua
        level *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);

        activeSources[sourceId] = baseIncreaseSpeed * level;
        timeSinceLastIncrease = 0f;
        isDecaying = false;
    }

    // chamado quando o jogador sai do FOV ou da zona suspeita ou então quando para de fazer a ação suspeita
    // para a subida mas não dá reset ao valor porque o decay trata disso com o delay
    public void StopIncreasingSuspicion(int sourceId)
    {
        activeSources.Remove(sourceId);
    }


    // usado para eventos discretos como um guarda ouvir um ruído ou ao tentar ouvir um telefonema que não deve
    // o amount deve ser um valor pequeno (ex: 0.05) para não dominar
    public void AddInstantSuspicion(float amount)
    {
        // atributo Sorte reduz a suspeita instantânea
        if (amount > 0)
        {
            amount *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
        }

        currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }


    // completar tarefas de trabalho baixa a suspeita (o jogador parece um funcionário normal), mas falhar ou completar incorretamente (por ex no entregar documento) sobe
    // amount é um multiplicador baseado na dificuldade da task
    public void ChangeSuspicionOnTaskComplete(float amount, bool doneCorrectly)
    {
        if (doneCorrectly)
            currentSuspicion = Mathf.Max(0f, currentSuspicion - amount);

        else
        {
            // atributo Sorte reduz a penalização por falhar tarefas
            amount *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);
        }

        // reset do decay para que a mudança seja processada imediatamente em vez de esperar pelo próximo ciclo de Update
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }


    // verifica se o ratio atual da barra ultrapassou algum threshold e, se o estado mudou, dispara o evento global
    // chamado no Update e sempre que o valor muda fora do Update (ex: ao completar uma task)
    private void CheckStateChange()
    {
        float ratio = currentSuspicion / maxSuspicion;

        SuspicionState newState;
        if (ratio >= 1f)
            newState = SuspicionState.Expulsion;
        else if (ratio >= 0.66f)
            newState = SuspicionState.Investigation;
        else if (ratio >= 0.33f)
            newState = SuspicionState.Attention;
        else
            newState = SuspicionState.None;

        // só dispara se o estado realmente mudou
        if (newState != currentState)
        {
            currentState = newState;

            // GameEvent.SuspicionStateChanged notifica o NPCManager, que por sua vez notifica todos os NPCs
            GameEvent.SuspicionStateChanged(newState);

            // expulsion Game Over -> dispara evento separado para que o GameManager possa reagir
            if (newState == SuspicionState.Expulsion)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.alarmExpulsion);
                GameEvent.GameOver();
            }
        }
    }

    public SuspicionState GetCurrentState()
    {
        return currentState;
    }
    public float GetSuspicionRatio()
    {
        return currentSuspicion / maxSuspicion;
    }

    // setter para o SaveManager restaurar a suspeita
    public void SetSuspicionDirect(float ratio)
    {
        currentSuspicion = ratio * maxSuspicion;
    }

    // repõe a suspeita, as fontes ativas e o estado atual a zero para que um "Novo Jogo" comece do zero
    public void ResetForNewGame()
    {
        currentSuspicion = 0f;
        activeSources.Clear();
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        currentState = SuspicionState.None;

        if (heartbeatSource != null && heartbeatSource.isPlaying)
            heartbeatSource.Stop();

        Debug.Log("[SuspicionManager] Estado reiniciado para um novo jogo.");
    }

    // de acordo com o níve da suspeita metemos o efeito de áudio do coração a correr ou não
    private void UpdateHeartbeat()
    {

        if (currentSuspicion > 0f)
        {
            if (!heartbeatSource.isPlaying)
            {
                heartbeatSource.clip = SoundManager.Instance.heartbeatPulse;
                heartbeatSource.loop = true;
                heartbeatSource.Play();
            }
            int stateIndex = (int)currentState; // enum: None=0, Attention=1, Investigation=2, Expulsion=3
            heartbeatSource.volume = SoundManager.Instance.heartbeatVolumeSteps[Mathf.Clamp(stateIndex, 0, SoundManager.Instance.heartbeatVolumeSteps.Length - 1)];

        }
        else
        {
            if (heartbeatSource.isPlaying)
                heartbeatSource.Stop();
        }
    }
}