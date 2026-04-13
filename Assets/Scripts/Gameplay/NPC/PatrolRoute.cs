using UnityEngine;
using System.Collections.Generic;
using static NPCScript;

public class PatrolRoute : MonoBehaviour {

    // tipos de NPC que podem usar esta rota — filtrado pelo GetRandomRoute do NPCManager
    // NPCs com assignedRoute ignoram este campo porque nunca chamam GetRandomRoute
    public NPCType[] allowedTypes;

    // probabilidade relativa de esta rota ser escolhida entre as compatíveis
    // ex: patrulha=1, descanso=0.3 -> guarda descansa ~23% das vezes
    [Range(0f, 1f)]
    public float probability = 1f;

    // segundos que o NPC espera em cada waypoint antes de avançar
    // útil para simular paragens naturais (ex: guarda a observar, rececionista a beber água)
    public float waitTimePerWaypoint = 0f;

    // se true, o NPC repete a rota do início ao chegar ao último waypoint, para sempre
    // usado em guardas em patrulha contínua
    // se false, chega ao fim e chama EnterPatrol para escolher rota nova
    public bool loopWaypoints = false;

    // se true, o NPC vai para o homeBase no fim da rota antes de escolher a próxima
    // requer que o NPCScript tenha homeBase definido
    // usado em rececionistas e visitantes que voltam à secretária/entrada depois de cada tarefa
    public bool returnHome = false;

    // marca esta rota como zona de descanso
    // o NPCManager.CanGuardRest verifica este flag para garantir que
    // só um guarda descansa de cada vez
    public bool isRestRoute = false;

    // preenchido automaticamente no Awake com os filhos diretos deste GameObject
    // filhos que tenham PatrolRoute são ignorados (ex: DESCANSO1 dentro de GUARDAS)
    [HideInInspector] public Transform[] waypoints;

    public int departmentID = 0; // por causa dos colegas rececionistas e chefes no piso executivo (0 = sem departamento)

    void Awake() {
        List<Transform> pts = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<PatrolRoute>() == null)
                pts.Add(child);
        }
        waypoints = pts.ToArray();
    }
}