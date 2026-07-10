using UnityEngine;
using System.Collections.Generic;
using static NPCScript;

// #my_code - Configuração e validação de rotas individuais: probabilidade, loop, descanso, departamento
public class PatrolRoute : MonoBehaviour {

    // Lista que filtra as entidades permitidas aquando da chamada aleatoria do gestor global de rotas.
    public NPCType[] allowedTypes;

    // Afina o balanco percentual face as alternativas. Da imenso jeito para definir um bias estatistico nalgumas zonas criticas.
    [Range(0f, 1f)]
    public float probability = 1f;

    public float waitTimePerWaypoint = 0f;

    // Mantem o script do navmesh a patinar ciclicamente nestes transforms especificos e ignora as conjeturas do EnterPatrol.
    public bool loopWaypoints = false;

    // Bloqueia o NPC no loop de retorno forçando a ida a origem pre gravada em vez de sortear um destino parvo no meio do corredor.
    public bool returnHome = false;

    // Tag para detetar a ocupacao. O NPCManager valida isto para impedir congregacoes de muitos segurancas num spot unico a passarem tempo.
    public bool isRestRoute = false;

    // Vetor invisivel carregado em runtime com os childs validos da arvore.
    [HideInInspector] public Transform[] waypoints;

    public int departmentID = 0;

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