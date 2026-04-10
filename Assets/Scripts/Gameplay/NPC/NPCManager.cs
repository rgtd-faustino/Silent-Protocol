using System.Collections.Generic;
using UnityEngine;
using static NPCScript;

public class NPCManager : MonoBehaviour {

    public static NPCManager Instance;

    public Transform player;

    // usada para broadcasts de suspeita e para verificar condições (CanReceptionistLeave, CanGuardRest)
    private List<NPCScript> activeNPCs = new List<NPCScript>();

    // rotas organizadas por piso — cada elemento tem um floor e um array de PatrolRoutes
    // o GetRandomRoute filtra por piso, tipo de NPC e outras condições
    [SerializeField] private FloorRoutes[] floorRoutes;

    [System.Serializable]
    public class FloorRoutes {
        public int floor;
        public PatrolRoute[] routes;
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    // devolve uma rota aleatória compatível com o tipo e piso do NPC
    // excludeRoute: evita repetir a rota anterior
    // excludeRest: ignora rotas de descanso (usado quando já há um guarda a descansar)
    public PatrolRoute GetRandomRoute(NPCType type, int floor, PatrolRoute excludeRoute = null, bool excludeRest = false) {
        List<PatrolRoute> compatible = new List<PatrolRoute>();

        for (int f = 0; f < floorRoutes.Length; f++) {
            if (floorRoutes[f].floor != floor) 
                continue;

            PatrolRoute[] routes = floorRoutes[f].routes;
            for (int i = 0; i < routes.Length; i++) {
                PatrolRoute route = routes[i];
                bool typeAllowed = false;

                for (int j = 0; j < route.allowedTypes.Length; j++) {
                    if (route.allowedTypes[j] == type) { 
                        typeAllowed = true; 
                        break; 
                    }
                }

                bool notExcluded = route != excludeRoute;
                bool restOk = !excludeRest || !route.isRestRoute;

                if (typeAllowed && notExcluded && restOk)
                    compatible.Add(route);
            }
        }

        if (compatible.Count == 0) 
            return null;

        float totalWeight = 0f;
        for (int i = 0; i < compatible.Count; i++)
            totalWeight += compatible[i].probability;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < compatible.Count; i++) {
            cumulative += compatible[i].probability;
            if (roll <= cumulative) return compatible[i];
        }
        return compatible[compatible.Count - 1];
    }

    // garante que pelo menos uma rececionista fica sempre na secretária
    // chamado no EnterPatrol de cada rececionista antes de sair
    public bool CanReceptionistLeave() {
        int atHome = 0;
        for (int i = 0; i < activeNPCs.Count; i++) {
            if (activeNPCs[i].npcType == NPCType.Receptionist && activeNPCs[i].isAtHome)
                atHome++;
        }
        return atHome > 1;
    }

    // garante que só um guarda descansa de cada vez
    // chamado no EnterPatrol do guarda quando a rota escolhida é isRestRoute
    public bool CanGuardRest() {
        for (int i = 0; i < activeNPCs.Count; i++) {
            if (activeNPCs[i].npcType == NPCType.Guard && activeNPCs[i].isResting)
                return false;
        }
        return true;
    }




    void OnEnable() => GameEvent.OnSuspicionStateChanged += OnSuspicionStateChanged;
    void OnDisable() => GameEvent.OnSuspicionStateChanged -= OnSuspicionStateChanged;

    public void RegisterNPC(NPCScript npc) {
        if (!activeNPCs.Contains(npc))
            activeNPCs.Add(npc);
    }

    public void UnregisterNPC(NPCScript npc) {
        activeNPCs.Remove(npc);
    }

    // recebido via GameEvent quando a SuspicionManager muda de estado
    // propaga a mudança a todos os NPCs ativos
    private void OnSuspicionStateChanged(SuspicionManager.SuspicionState state) {
        foreach (NPCScript npc in activeNPCs)
            npc.OnGlobalSuspicionChanged(state);
    }

    public void SetAllPatrolling(bool patrolling) {
        foreach (NPCScript npc in activeNPCs)
            npc.SetPatrolling(patrolling);
    }
}