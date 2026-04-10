using UnityEngine;
using UnityEngine.UI;

public class SuspicionManager : MonoBehaviour {

    public static SuspicionManager Instance;

    [SerializeField] private float maxSuspicion = 1f;
    private float currentSuspicion = 0f;

    // baseIncreaseSpeed: velocidade base a que a suspeita sobe por segundo quando há uma fonte ativa (ex: NPC a ver o jogador).
    // multiplicada pelo "level" da fonte (1, 1.5 ou 2) para fontes mais graves
    // decayDelay: segundos sem descer mais depois da fonte parar antes da suspeita começar a baixar
    // decaySpeed: velocidade a que a suspeita baixa por segundo durante o decay
    [SerializeField] private float baseIncreaseSpeed = 0.1f;
    [SerializeField] private float decayDelay = 10f;
    [SerializeField] private float decaySpeed = 0.03f;

    // currentIncreaseRate: taxa de subida atual (0 se não há fonte ativa)
    // timeSinceLastIncrease: contador para o decayDelay
    // isDecaying: flag que ativa o decay após o delay expirar
    private float currentIncreaseRate = 0f;
    private float timeSinceLastIncrease = 0f;
    private bool isDecaying = false;

    // estado atual (None/Attention/Investigation/Expulsion)
    // guardado para detetar mudanças e disparar o evento apenas quando muda
    private SuspicionState currentState = SuspicionState.None;


    // None        — comportamento normal
    // Attention   — NPCs observam mais (>33% da barra)
    // Investigation — guardas aumentam patrulhas (>66%)
    // Expulsion   — Game Over (100%)
    public enum SuspicionState {
        None,
        Attention,
        Investigation,
        Expulsion
    }

    public enum SuspicionSource {
        NPCSight,       // NPC vê o jogador numa zona suspeita
        RestrictedArea, // jogador está numa zona restrita
        Camera,         // acesso excessivo a câmaras
        Noise,          // barulho à noite
        TerminalAccess,  // acesso a terminais fora do posto de trabalho
        DocumentMisfiled   // documento arquivado no departamento errado
    }


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;

    }


    void Update() {
        if (currentIncreaseRate > 0) {
            // há uma fonte ativa -> a suspeita sobe.
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + currentIncreaseRate * Time.deltaTime);

            // reset do contador de decay -> enquanto há fonte ativa o timer não avança
            timeSinceLastIncrease = 0f;
            isDecaying = false;

        } else {
            // sem fonte ativa —> conta o tempo antes do decay
            timeSinceLastIncrease += Time.deltaTime;

            if (timeSinceLastIncrease >= decayDelay)
                isDecaying = true;

            // baixa a suspeita gradualmente após o delay
            if (isDecaying && currentSuspicion > 0) {
                currentSuspicion = Mathf.Max(0f,
                    currentSuspicion - decaySpeed * Time.deltaTime);

                if (currentSuspicion <= 0)
                    isDecaying = false;
            }
        }

        CheckStateChange();
    }


    // chamado pelo NPCScript (e futuramente por câmaras, áreas restritas, etc.)
    // level vai de 1 a 3 — representa a gravidade da situação
    public void IncreaseSuspicion(float level, SuspicionSource source = SuspicionSource.NPCSight) {
        if (level < 1 || level > 3) 
            return; // valores fora do intervalo são ignorados

        currentIncreaseRate = baseIncreaseSpeed * level;
        timeSinceLastIncrease = 0f;
        isDecaying = false;
    }

    // chamado pelo NPCScript quando o jogador sai do FOV ou da zona suspeita
    // pára a subida mas não dá reset o valor porque o decay trata disso com o delay
    public void StopIncreasingSuspicion() {
        currentIncreaseRate = 0f;
    }


    // completar tarefas de trabalho baixa a suspeita (o jogador parece um funcionário normal), mas falhar ou completar incorretamente sobe
    // amount é um multiplicador baseado na dificuldade da task (definido no TaskManager: Small=0.1, Medium=0.25, Major=0.5).
    public void ChangeSuspicionOnTaskComplete(float amount, bool doneCorrectly) {
        if (doneCorrectly)
            currentSuspicion = Mathf.Max(0f, currentSuspicion - amount);
        else
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);

        // reset do decay para que a mudança seja processada imediatamente em vez de esperar pelo próximo ciclo de Update.
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }


    // verifica se o ratio atual da barra cruzou algum threshold e, se o estado mudou, dispara o evento global
    // chamado no Update e sempre que o valor muda fora do Update (ex: ao completar uma task).
    private void CheckStateChange() {
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
        if (newState != currentState) {
            currentState = newState;

            // GameEvent.SuspicionStateChanged notifica o NPCManager, que por sua vez notifica todos os NPCs
            GameEvent.SuspicionStateChanged(newState);

            // expulsion é Game Over —> dispara evento separado para que o GameManager possa reagir.
            if (newState == SuspicionState.Expulsion)
                GameEvent.GameOver();
        }
    }

    // permite que outros scripts leiam o estado atual sem aceder diretamente ao slider
    public SuspicionState GetCurrentState() {
        return currentState;
    }
    public float GetSuspicionRatio() {
        return currentSuspicion / maxSuspicion;
    }
}