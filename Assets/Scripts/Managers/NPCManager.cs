using System.Collections.Generic;
using UnityEngine;
using static NPCScript;

// #my_code - Orquestração centralizada de rotas e eventos (reunião, spawn) para todos os pisos
public class NPCManager : MonoBehaviour {

    public static NPCManager Instance;

    public Transform player;

    // mantemos esta lista global atualizada para conseguirmos fazer broadcasts quando a suspeita sobe, 
    // e também para validar regras estritas tipo não deixar a receção vazia
    private List<NPCScript> activeNPCs = new List<NPCScript>();

    // definimos as rotas no inspector separadas por piso. o GetRandomRoute depois filtra por estes arrays para evitar alocar memória nova a cada sorteio
    [SerializeField] private FloorRoutes[] floorRoutes;

    [SerializeField] private NPCScript bossD1, colega1D1, colega2D1;
    [SerializeField] private NPCScript bossD2, colega1D2, colega2D2;
    [SerializeField] private NPCScript bossD3, colega1D3, colega2D3;

    // isolamos as rotas de reunião das patrulhas normais para não corrermos o risco de um NPC ir parar à sala de reuniões aleatoriamente durante o dia
    [SerializeField] private PatrolRoute meetingRouteBossD1, meetingRouteColega1D1, meetingRouteColega2D1;
    [SerializeField] private PatrolRoute meetingRouteBossD2, meetingRouteColega1D2, meetingRouteColega2D2;
    [SerializeField] private PatrolRoute meetingRouteBossD3, meetingRouteColega1D3, meetingRouteColega2D3;

    private void Update()
    {

    }

    // injetamos a rota nos NPCs diretamente quando bate a hora da reunião. 
    // usamos o ForceRoute para eles ignorarem o que estavam a fazer e marcharem em fila para a sala
    public void TriggerMeeting() {
        GameEvent.MeetingStarted();
        bossD1.ForceRoute(meetingRouteBossD1);
        colega1D1.ForceRoute(meetingRouteColega1D1);
        colega2D1.ForceRoute(meetingRouteColega2D1);
        bossD2.ForceRoute(meetingRouteBossD2);
        colega1D2.ForceRoute(meetingRouteColega1D2);
        colega2D2.ForceRoute(meetingRouteColega2D2);
        bossD3.ForceRoute(meetingRouteBossD3);
        colega1D3.ForceRoute(meetingRouteColega1D3);
        colega2D3.ForceRoute(meetingRouteColega2D3);
    }


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

    // este algoritmo procura uma rota compatível cruzando os requisitos todos (piso, tipo, departamento).
    // metemos pesos de probabilidade nas rotas para não parecer mecânico e o NPC preferir as rotas principais em vez de ir sempre à casa de banho
    public PatrolRoute GetRandomRoute(NPCType type, int floor, int departmentID, PatrolRoute excludeRoute = null, bool excludeRest = false) {
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
                bool deptOk = route.departmentID == 0 || route.departmentID == departmentID;

                if (typeAllowed && notExcluded && restOk && deptOk)
                    compatible.Add(route);
            }
        }

        // salvaguarda parva: se as exclusões derem cabo das opções todas, tentamos outra vez à força bruta sem filtros para o NPC não encravar no sítio
        if (compatible.Count == 0 && excludeRoute != null)
            return GetRandomRoute(type, floor, departmentID, null, excludeRest);

        if (compatible.Count == 0)
            return null;

        // sorteio baseado na roleta de probabilidades para permitir rotas mais raras
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

    // força a regra de manter sempre alguém na front desk para o jogo fazer sentido narrativamente
    public bool CanReceptionistLeave() {
        int atHome = 0;
        for (int i = 0; i < activeNPCs.Count; i++) {
            if (activeNPCs[i].npcType == NPCType.Receptionist && activeNPCs[i].isAtHome)
                atHome++;
        }
        return atHome > 1;
    }

    // regra de descanso dos guardas para não ficarem todos a beber café ao mesmo tempo e arruinarem o stealth
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

    // o SuspicionManager avisa-nos que deu barraca e nós empurramos esse estado para cada boneco individual reagir
    private void OnSuspicionStateChanged(SuspicionManager.SuspicionState state) {
        foreach (NPCScript npc in activeNPCs)
            npc.OnGlobalSuspicionChanged(state);
    }

    public void SetAllPatrolling(bool patrolling) {
        foreach (NPCScript npc in activeNPCs)
            npc.SetPatrolling(patrolling);
    }

    // otimização básica: o GameManager chama isto ao mudar de piso para desligar a navegação e a física dos gajos que ficaram noutros andares
    public void SetActiveFloor(int floor) {
        foreach (NPCScript npc in activeNPCs)
            npc.SetFloorActive(npc.currentFloor == floor);
    }
}