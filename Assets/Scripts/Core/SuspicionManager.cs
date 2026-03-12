using UnityEngine;
using UnityEngine.UI;
using static NPCScript;
public class SuspicionManager : MonoBehaviour {
    public static SuspicionManager Instance;
    [SerializeField] private Slider suspicionSlider;
    [SerializeField] private float baseIncreaseSpeed = 0.1f; // velocidade base por segundo
    [SerializeField] private float decayDelay = 10f; // tempo em segundos antes de começar decay
    [SerializeField] private float decaySpeed = 0.03f; // velocidade de redução (mais lenta que nível 1 (0.1))

    private float currentIncreaseRate = 0f;
    private float timeSinceLastIncrease = 0f; // timer desde última detecção
    private bool isDecaying = false;

    // estes eventos funcionam como listas de funções e quando são chamados todas as funções são chamadas
    public static event System.Action<SuspicionState> OnStateChanged; // so aceita funções que tenham (SuspicionState state) como parâmetro
    private SuspicionState currentState = SuspicionState.None;

    public enum SuspicionState {
        None,
        Attention,
        Investigation,
        Expulsion
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {

    }

    void Update() {
        // se o jogador estiver a ficar suspeito aumentamos de acordo com o rate atual (mudado de acordo com a distancia ao NPC por enquanto
        if (currentIncreaseRate > 0) {
            suspicionSlider.value += currentIncreaseRate * Time.deltaTime;

            // se ja chegou ao limite máximo então o rate fica a 0
            if (suspicionSlider.value >= suspicionSlider.maxValue) {
                suspicionSlider.value = suspicionSlider.maxValue;
                currentIncreaseRate = 0f;
            }

            // damos reset à ultima vez desde q o player n aumentava o nivel de suspeito
            timeSinceLastIncrease = 0f;
            isDecaying = false;

            // se o jogador não estiver a ficar suspeito então vemos de podemos diminuir
        } else {
            timeSinceLastIncrease += Time.deltaTime; // aumentamos a duração de tempo desde que a ultima vez que ficou mais sus

            if (timeSinceLastIncrease >= decayDelay && !isDecaying) {
                isDecaying = true; // se já passou mais do que o tempo necessário então começamos a baixar lhe o nível de suspeita
            }

            // se já tiver a 0 não fazemos nada
            if (isDecaying && suspicionSlider.value > 0) {
                suspicionSlider.value -= decaySpeed * Time.deltaTime;

                // não deixamos ir para baixo de 0 (damos reset para 0)
                if (suspicionSlider.value <= 0) {
                    suspicionSlider.value = 0;
                    isDecaying = false;
                }
            }
        }

        CheckStateChange();
    }

    private void CheckStateChange() {
        float ratio = suspicionSlider.value / suspicionSlider.maxValue;

        SuspicionState newState;

        if (ratio >= 1f)
            newState = SuspicionState.Expulsion;
        else if (ratio >= 0.66f)
            newState = SuspicionState.Investigation;
        else if (ratio >= 0.33f)
            newState = SuspicionState.Attention;
        else newState = SuspicionState.None;

        if (newState != currentState) {
            currentState = newState;
            OnStateChanged.Invoke(currentState); // invoke é o que chama o evento
        }
    }


    public void IncreaseSuspicion(float level) {
        if (level < 1 || level > 3) 
            return; // niveis de velocidade de ganhar suspeita

        currentIncreaseRate = baseIncreaseSpeed * level;

        // da reset ao decay quando deteta novamente
        timeSinceLastIncrease = 0f;
        isDecaying = false;
    }

    public void StopIncreasingSuspicion() {
        currentIncreaseRate = 0f;
    }
}