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

    // os NPCs que vao para a reunião
    [SerializeField] private NPCScript bossD1, colega1D1, colega2D1;
    [SerializeField] private NPCScript bossD2, colega1D2, colega2D2;
    [SerializeField] private NPCScript bossD3, colega1D3, colega2D3;

    // rota para a sala de reuniões de cada NPC
    [SerializeField] private PatrolRoute meetingRouteBossD1, meetingRouteColega1D1, meetingRouteColega2D1;
    [SerializeField] private PatrolRoute meetingRouteBossD2, meetingRouteColega1D2, meetingRouteColega2D2;
    [SerializeField] private PatrolRoute meetingRouteBossD3, meetingRouteColega1D3, meetingRouteColega2D3;

    private void Update()
    {

    }

    public void TriggerMeeting() {
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

    // devolve uma rota aleatória compatível com o tipo e piso do NPC
    // excludeRoute: evita repetir a rota anterior
    // excludeRest: ignora rotas de descanso (usado quando já há um guarda a descansar)
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

        // se não encontrou nenhuma rota compatível após excluir a rota anterior, tenta novamente sem exclusão
        // isto acontece quando o pool do NPC só tem uma rota, excluir a única opção devolvia null e dava erro
        // ao repetir sem exclusão o NPC fica na mesma rota em vez de ficar parado
        if (compatible.Count == 0 && excludeRoute != null)
            return GetRandomRoute(type, floor, departmentID, null, excludeRest);

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

    public void SetActiveFloor(int floor) {
        foreach (NPCScript npc in activeNPCs)
            npc.SetFloorActive(npc.currentFloor == floor);
    }
}