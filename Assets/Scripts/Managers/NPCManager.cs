using System.Collections.Generic;
using UnityEngine;
using static NPCScript;

// #my_code - Orquestração centralizada de rotas e eventos (reunião, spawn) para todos os pisos
public class NPCManager : MonoBehaviour {

    public static NPCManager Instance;

    public Transform player;

    // mantemos esta lista global atualizada para conseguirmos fazer broadcasts quando a suspeita sobe e também para validar regras estritas tipo não deixar a receção vazia
    private List<NPCScript> activeNPCs = new List<NPCScript>();

    // definimos que rotas podem ser percorridas por piso, porque cada piso tem as suas próprias rotas
    [SerializeField] private FloorRoutes[] floorRoutes;

    // NPC que fazem parte do evento das reuniões para lhes atribuirmos as rotas de ir para a reunião
    [SerializeField] private NPCScript bossD1, colega1D1, colega2D1;
    [SerializeField] private NPCScript bossD2, colega1D2, colega2D2;
    [SerializeField] private NPCScript bossD3, colega1D3, colega2D3;

    // isolamos as rotas de reunião das patrulhas normais para não corrermos o risco de um NPC ir parar à sala de reuniões aleatoriamente durante o dia
    [SerializeField] private PatrolRoute meetingRouteBossD1, meetingRouteColega1D1, meetingRouteColega2D1;
    [SerializeField] private PatrolRoute meetingRouteBossD2, meetingRouteColega1D2, meetingRouteColega2D2;
    [SerializeField] private PatrolRoute meetingRouteBossD3, meetingRouteColega1D3, meetingRouteColega2D3;

    [System.Serializable]
    public class FloorRoutes {
        public int floor;
        public PatrolRoute[] routes;
    }



    // injetamos a rota nos NPCs diretamente quando chega a hora da reunião
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



    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
    }

    // este algoritmo procura uma rota compatível cruzando os requisitos todos (piso, tipo, departamento, excluir a rota atual e se é para exlcuir as rotas de descanso)
    public PatrolRoute GetRandomRoute(NPCType type, int floor, int departmentID, PatrolRoute excludeRoute = null, bool excludeRest = false) {
        List<PatrolRoute> compatible = new List<PatrolRoute>();

        // apanhamos as rotas de cada piso
        for (int f = 0; f < floorRoutes.Length; f++) {
            // só queremos saber das rotas do piso onde o NPC se encontra
            if (floorRoutes[f].floor != floor)
                continue;

            PatrolRoute[] routes = floorRoutes[f].routes; // apanhamos as rotas deste piso especificamente
            for (int i = 0; i < routes.Length; i++) {
                PatrolRoute route = routes[i];
                bool typeAllowed = false;

                // para esta rota especificamente procuramos saber se este NPC a pode percorrer
                for (int j = 0; j < route.allowedTypes.Length; j++) {
                    if (route.allowedTypes[j] == type) {
                        typeAllowed = true;
                        break;
                    }
                }

                bool notExcluded = route != excludeRoute;
                bool restOk = !excludeRest || !route.isRestRoute;
                bool deptOk = route.departmentID == 0 || route.departmentID == departmentID;

                // se o NPC for permitido, se não for a rota atual, se a rota for de descanso e o NPC não a excluir (ou se não for rota de descanso)
                // e se a rota for para um departamento específico e este NPC for desse mesmo (ou não for para um dep especificamente) então permitimos
                // que a rota seja adicionada para as possíveis
                if (typeAllowed && notExcluded && restOk && deptOk)
                    compatible.Add(route);
            }
        }

        // se as exclusões por acaso derem cabo das opções todas, tentamos outra vez à força sem filtros para o NPC não encravar no sítio
        if (compatible.Count == 0 && excludeRoute != null)
            return GetRandomRoute(type, floor, departmentID, null, excludeRest);

        if (compatible.Count == 0)
            return null;

        // somamos os pesos todos das rotas compatíveis
        float totalWeight = 0f;
        for (int i = 0; i < compatible.Count; i++)
            totalWeight += compatible[i].probability;

        // tiramos à sorte um número aleatório entre 0 e o peso total
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        // percorremos as rotas por ordem e vamos somando os pesos
        // assim que essa soma ultrapassar o valor escolhido ao acaso encontrámos a fatia que calhou e devolvemos essa rota
        // rotas com probability maior têm fatias maiores, logo saem mais vezes
        for (int i = 0; i < compatible.Count; i++) {
            cumulative += compatible[i].probability;
            if (roll <= cumulative) 
                return compatible[i];
        }

        // com floats pode haver erro de arredondamento e o cumulative nunca chegar mesmo a ultrapassar o roll
        // por isso devolvemos sempre a última rota em vez de correr o risco de sair null
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

    // regra de descanso dos guardas para não ficarem todos a descansar ao mesmo tempo
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

    // o SuspicionManager avisa o NPC que a suspeita do jogador foi alterada para cada NPC individual poder reagir
    private void OnSuspicionStateChanged(SuspicionManager.SuspicionState state) {
        foreach (NPCScript npc in activeNPCs)
            npc.OnGlobalSuspicionChanged(state);
    }


    // o GameManager chama isto ao mudar de piso para desligar a navegação e a física dos NPC que ficaram noutros andares para poupar performance
    public void SetActiveFloor(int floor) {
        foreach (NPCScript npc in activeNPCs)
            npc.SetFloorActive(npc.currentFloor == floor);
    }
}