using UnityEngine;
using System.Collections.Generic;
using static NPCScript;

// #my_code - Configuração e validação de rotas individuais: probabilidade, loop, descanso, departamento
public class PatrolRoute : MonoBehaviour {

    // quais NPC são permitidos percorrer os waypoints de cada rota
    public NPCType[] allowedTypes;

    // para acrescentar uma certa imprevisibilidade ao jogo fazemos com que cada waypoint possa ser ignorado e passado a frente de acordo com uma probabilidade
    [Range(0f, 1f)]
    public float probability = 1f;

    public float waitTimePerWaypoint = 0f; // quanto tempo o NPC fica em cada waypoint

    // isto mantém o NPC a percorrer a rota em loop, por exemplo os guardas a vaguear
    public bool loopWaypoints = false;

    // esta variável faz com que o NPC volte ao seu ponto original, por exemplo quando um NPC spawna num piso e volta para o elevador no fim
    public bool returnHome = false;

    // NPC estacionários por exemplo guardas e rececionistas que têm de ficar no mínimo sempre um a trabalhar, então se uma rota for marcada como de descanso
    // o código apenas permite que um NPC descanse de cada vez
    public bool isRestRoute = false;

    // conjunto dos waypoints a percorrer
    [HideInInspector] public Transform[] waypoints;

    public int departmentID = 0; // rotas específicas para NPC de certos departamentos, nomeadamente comer e beber água dentro do seu dep

    // como atribuímos o gameobject parent que tem todos os waypoints gameobjects como filhos percorremos todos eles e vamos atribuindo dinamicamente
    // em vez de atribuir no inspetor cada um
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