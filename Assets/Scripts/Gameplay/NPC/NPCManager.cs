using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour {

    public static NPCManager Instance;

    public Transform player;
    private List<NPCScript> activeNPCs = new List<NPCScript>();


    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // OnEnable e OnDisable para subscrever/desinscrever eventos
    void OnEnable() => GameEvent.OnSuspicionStateChanged += OnSuspicionStateChanged;
    void OnDisable() => GameEvent.OnSuspicionStateChanged -= OnSuspicionStateChanged;


    // chamados pelo NPCScript no seu Start() e OnDestroy().
    // o Contains() em register serve para não adicionarmos o mesmo NPC duas vezes
    public void RegisterNPC(NPCScript npc) {
        if (!activeNPCs.Contains(npc))
            activeNPCs.Add(npc);
    }

    public void UnregisterNPC(NPCScript npc) {
        activeNPCs.Remove(npc);
    }


    // chamado automaticamente pelo GameEvent quando o SuspicionManager muda de estado
    // distribui o novo estado a todos os NPCs registados e depois decidem individualmente como reagir com base no seu tipo
    // (guarda reage de forma diferente de colega)
    private void OnSuspicionStateChanged(SuspicionManager.SuspicionState state) {
        foreach (var npc in activeNPCs)
            npc.OnGlobalSuspicionChanged(state);
    }


    // ponto de controlo centralizado para parar/reiniciar todas as patrulhas de uma vez
    public void SetAllPatrolling(bool patrolling) {
        foreach (var npc in activeNPCs)
            npc.SetPatrolling(patrolling);
    }
}