// #my_code - Sistema duplo de suspeita: company awareness + suspeita individual por fonte
using System.Collections.Generic;
using UnityEngine;

public class SuspicionManager : MonoBehaviour {

    public static SuspicionManager Instance;

    AudioSource heartbeatSource;

    [SerializeField] private float maxSuspicion = 1f;
    private float currentSuspicion = 0f;

    [SerializeField] private float baseIncreaseSpeed = 0.1f;
    // tempo de colldown até a malta da empresa se esquecer que algo estranho aconteceu e a barra de suspeita começar a descer
    [SerializeField] private float decayDelay = 10f;
    [SerializeField] private float decaySpeed = 0.03f;

    // um dicionário porreiro para rastrear quem está a ver o jogador em simultâneo (câmaras, npcs). usamos o id da entidade como chave para não duplicar somas.
    private Dictionary<int, float> activeSources = new Dictionary<int, float>();
    private float timeSinceLastIncrease = 0f;
    private bool isDecaying = false;

    private SuspicionState currentState = SuspicionState.None;

    public enum SuspicionState {
        None,
        Attention,
        Investigation,
        Expulsion
    }

    public enum SuspicionSource {
        NPCSight,
        RestrictedArea,
        Camera,
        Noise,
        TerminalAccess,
        DocumentMisfiled,
        Hacking
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    private void Start() {
        heartbeatSource = SoundManager.Instance.heartbeatSource;
    }

    void Update() {
        float totalRate = 0f;
        foreach (float value in activeSources.Values)
            totalRate += value;

        if (totalRate > 0f) {
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + totalRate * Time.deltaTime);
            timeSinceLastIncrease = 0f;
            isDecaying = false;

        } else {
            timeSinceLastIncrease += Time.deltaTime;

            if (timeSinceLastIncrease >= decayDelay)
                isDecaying = true;

            if (isDecaying && currentSuspicion > 0) {
                currentSuspicion = Mathf.Max(0f,
                    currentSuspicion - decaySpeed * Time.deltaTime);

                if (currentSuspicion <= 0)
                    isDecaying = false;
            }
        }

        CheckStateChange();
        UpdateHeartbeat();
    }

    // este método conecta-se ao field of view dos NPCs e câmaras.
    // o atributo sorte em PlayerStats foi injetado aqui para que a build do jogador abrande o crescimento da barra e dê mais margem de manobra num stealth agressivo
    public void IncreaseSuspicion(float level, int sourceId, SuspicionSource source = SuspicionSource.NPCSight) {
        if (level < 1 || level > 3)
            return;

        if (PlayerStats.Instance != null) {
            level *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
        }

        activeSources[sourceId] = baseIncreaseSpeed * level;
        timeSinceLastIncrease = 0f;
        isDecaying = false;
    }

    public void StopIncreasingSuspicion(int sourceId) {
        activeSources.Remove(sourceId);
    }

    // usamos isto para picos únicos de suspeita (exemplo: barulhos ou encontrar ficheiros proibidos), assim evitamos gerir timers no dicionário para coisas instantâneas
    public void AddInstantSuspicion(float amount) {
        if (PlayerStats.Instance != null && amount > 0) {
            amount *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
        }

        currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }

    // misturámos o lado corporativo com o stealth. fazer o trabalho disfarça e diminui a suspeita, falhar as tarefas aumenta-a
    public void ChangeSuspicionOnTaskComplete(float amount, bool doneCorrectly) {
        if (doneCorrectly)
            currentSuspicion = Mathf.Max(0f, currentSuspicion - amount);
        else {
            if (PlayerStats.Instance != null) {
                amount *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
            }
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);
        }

        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }

    // optámos por notificar a mudança de estado apenas quando o limite cruza o patamar e não em todos os frames
    // isto poupa imenso porque não forçamos os NPCs a repensar as rotinas a cada micro-aumento
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

        if (newState != currentState) {
            currentState = newState;

            GameEvent.SuspicionStateChanged(newState);

            if (newState == SuspicionState.Expulsion) {
                SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.alarmExpulsion);
                GameEvent.GameOver();
            }
        }
    }

    public SuspicionState GetCurrentState() {
        return currentState;
    }

    public float GetSuspicionRatio() {
        return currentSuspicion / maxSuspicion;
    }

    public void SetSuspicionDirect(float ratio) {
        currentSuspicion = ratio * maxSuspicion;
    }

    // metemos o som a variar consoante o estado discreto (0-3) em vez de ajustar o pitch linearmente
    // é um design sonoro melhor porque avisa o jogador dos patamares cruciais de stealth de forma audível
    private void UpdateHeartbeat() {

        if (currentSuspicion > 0f) {
            if (!heartbeatSource.isPlaying) {
                heartbeatSource.clip = SoundManager.Instance.heartbeatPulse;
                heartbeatSource.loop = true;
                heartbeatSource.Play();
            }
            int stateIndex = (int)currentState;
            heartbeatSource.volume = SoundManager.Instance.heartbeatVolumeSteps[Mathf.Clamp(stateIndex, 0, SoundManager.Instance.heartbeatVolumeSteps.Length - 1)];

        } else {
            if (heartbeatSource.isPlaying)
                heartbeatSource.Stop();
        }
    }
}
