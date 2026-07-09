// #my_code - Sistema duplo de suspeita: company awareness + suspeita individual por fonte
﻿using System.Collections.Generic;
using UnityEngine;

public class SuspicionManager : MonoBehaviour {

    public static SuspicionManager Instance;

    AudioSource heartbeatSource;

    [SerializeField] private float maxSuspicion = 1f;
    private float currentSuspicion = 0f;

    // baseIncreaseSpeed: velocidade base a que a suspeita sobe por segundo quando h� uma fonte ativa (ex: NPC a ver o jogador).
    // multiplicada pelo "level" da fonte (1, 1.5 ou 2) para fontes mais graves
    // decayDelay: segundos sem descer mais depois da fonte parar antes da suspeita come�ar a baixar
    // decaySpeed: velocidade a que a suspeita baixa por segundo durante o decay
    [SerializeField] private float baseIncreaseSpeed = 0.1f;
    [SerializeField] private float decayDelay = 10f;
    [SerializeField] private float decaySpeed = 0.03f;

    // timeSinceLastIncrease: contador para o decayDelay
    // isDecaying: flag que ativa o decay ap�s o delay expirar
    private Dictionary<int, float> activeSources = new Dictionary<int, float>();
    private float timeSinceLastIncrease = 0f;
    private bool isDecaying = false;

    // estado atual (None/Attention/Investigation/Expulsion)
    // guardado para detetar mudan�as e disparar o evento apenas quando muda
    private SuspicionState currentState = SuspicionState.None;


    // None          � comportamento normal
    // Attention     � NPCs observam mais (>33% da barra)
    // Investigation � guardas aumentam patrulhas (>66%)
    // Expulsion     � Game Over (100%)
    public enum SuspicionState {
        None,
        Attention,
        Investigation,
        Expulsion
    }

    public enum SuspicionSource {
        NPCSight,           // NPC v� o jogador numa zona suspeita
        RestrictedArea,     // jogador est� numa zona restrita
        Camera,             // acesso excessivo a c�maras
        Noise,              // barulho � noite (guarda ouviu o jogador)
        TerminalAccess,     // acesso a terminais fora do posto de trabalho
        DocumentMisfiled,    // documento arquivado no departamento errado
        Hacking // hackear as camaras para lhes aceder 
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
        // apanhamos a suspeita total de todas as fontes
        foreach (float value in activeSources.Values) 
            totalRate += value;

        // h� uma fonte ativa -> a suspeita sobe
        if (totalRate > 0f) {
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + totalRate * Time.deltaTime);
            // reset do contador de decay -> enquanto h� fonte ativa o timer n�o avan�a
            timeSinceLastIncrease = 0f;
            isDecaying = false;

        } else {
            // sem fonte ativa -> conta o tempo antes do decay
            timeSinceLastIncrease += Time.deltaTime;

            if (timeSinceLastIncrease >= decayDelay)
                isDecaying = true;

            // baixa a suspeita gradualmente ap�s o delay
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


    // chamado pelo NPCScript (e futuramente por cmaras, reas restritas, etc.)
    // level vai de 1 a 3  representa a gravidade da situao
    public void IncreaseSuspicion(float level, int sourceId, SuspicionSource source = SuspicionSource.NPCSight) {
        if (level < 1 || level > 3)
            return; // valores fora do intervalo so ignorados

        // Atributo Sorte: Reduz a taxa de ganho de suspeita contínua
        if (PlayerStats.Instance != null) {
            level *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
        }

        activeSources[sourceId] = baseIncreaseSpeed * level;
        timeSinceLastIncrease = 0f;
        isDecaying = false;
    }

    // chamado pelo NPCScript quando o jogador sai do FOV ou da zona suspeita.
    // p�ra a subida mas n�o d� reset o valor porque o decay trata disso com o delay.
    public void StopIncreasingSuspicion(int sourceId) {
        activeSources.Remove(sourceId);
    }


    // aumento pontual (one-shot) da suspeita � n�o � rate-based, n�o � cancelado pelo StopIncreasingSuspicion.
    // usado para eventos discretos como um guarda ouvir um ru�do ou o jogador entrar brevemente numa zona proibida.
    // amount deve ser um valor pequeno (ex: 0.05) para n�o dominar a mec�nica de vis�o.
    public void AddInstantSuspicion(float amount) {
        // Atributo Sorte: Reduz a suspeita instantânea
        if (PlayerStats.Instance != null && amount > 0) {
            amount *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
        }

        currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }


    // completar tarefas de trabalho baixa a suspeita (o jogador parece um funcion�rio normal), mas falhar ou completar incorretamente sobe.
    // amount � um multiplicador baseado na dificuldade da task (definido no TaskManager: Small=0.1, Medium=0.25, Major=0.5).
    public void ChangeSuspicionOnTaskComplete(float amount, bool doneCorrectly) {
        if (doneCorrectly)
            currentSuspicion = Mathf.Max(0f, currentSuspicion - amount);
        else {
            // Atributo Sorte: Reduz a penalização por falhar tarefas
            if (PlayerStats.Instance != null) {
                amount *= (1f - PlayerStats.Instance.GetSorte() * 0.03f);
            }
            currentSuspicion = Mathf.Min(maxSuspicion, currentSuspicion + amount);
        }

        // reset do decay para que a mudan�a seja processada imediatamente em vez de esperar pelo pr�ximo ciclo de Update.
        timeSinceLastIncrease = 0f;
        isDecaying = false;
        CheckStateChange();
    }


    // verifica se o ratio atual da barra cruzou algum threshold e, se o estado mudou, dispara o evento global.
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

        // s dispara se o estado realmente mudou
        if (newState != currentState) {
            currentState = newState;

            // GameEvent.SuspicionStateChanged notifica o NPCManager, que por sua vez notifica todos os NPCs
            GameEvent.SuspicionStateChanged(newState);

            // expulsion  Game Over -> dispara evento separado para que o GameManager possa reagir.
            if (newState == SuspicionState.Expulsion) {
                SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.alarmExpulsion);
                GameEvent.GameOver();
            }
        }
    }

    // permite que outros scripts leiam o estado atual sem aceder diretamente ao slider
    public SuspicionState GetCurrentState() {
        return currentState;
    }
    public float GetSuspicionRatio() {
        return currentSuspicion / maxSuspicion;
    }

    // setter para o SaveManager restaurar a suspeita
    public void SetSuspicionDirect(float ratio) {
        currentSuspicion = ratio * maxSuspicion;
    }

    private void UpdateHeartbeat() {

        if (currentSuspicion > 0f) {
            if (!heartbeatSource.isPlaying) {
                heartbeatSource.clip = SoundManager.Instance.heartbeatPulse;
                heartbeatSource.loop = true;
                heartbeatSource.Play();
            }
            int stateIndex = (int)currentState; // enum: None=0, Attention=1, Investigation=2, Expulsion=3
            heartbeatSource.volume = SoundManager.Instance.heartbeatVolumeSteps[Mathf.Clamp(stateIndex, 0, SoundManager.Instance.heartbeatVolumeSteps.Length - 1)];

        } else {
            if (heartbeatSource.isPlaying)
                heartbeatSource.Stop();
        }
    }
}
