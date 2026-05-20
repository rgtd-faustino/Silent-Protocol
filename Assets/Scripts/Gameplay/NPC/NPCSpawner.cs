using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour {
    [SerializeField] private List<GameObject> npcPrefabs = new List<GameObject>();

    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private GameObject npcPrefab1;
    [SerializeField] private GameObject npcPrefab2;
    [SerializeField] private GameObject npcPrefab3;
    // ponto de spawn na cena (porta de entrada, receo, etc.)
    private Transform spawnPoint;

    // rota fixa atribuda a cada NPC spawnado por este spawner
    // ex: CAMINHO1 para colegas, DEAMBULAR para visitantes
    // se null, o NPC usa o sistema aleatrio normal do NPCManager
    [SerializeField] private PatrolRoute assignedRoute;

    [SerializeField] private PatrolRoute startRoute;

    // quantos NPCs deste spawner podem existir na cena ao mesmo tempo
    [SerializeField] private int maxActive = 3;

    // segundos entre cada tentativa de spawn
    [SerializeField] private float spawnInterval = 30f;

    // contador interno  sobe no spawn, desce quando um NPC  destrudo via OnNPCDestroyed
    private int currentActive = 0;


    void Start() {
        spawnPoint = transform;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine() {
        while (true) {
            yield return new WaitForSeconds(TimeManager.Instance.ToRealSeconds(spawnInterval));

            if (currentActive < maxActive && npcPrefabs.Count > 0) {
                currentActive++;
                GameObject obj = Instantiate(npcPrefabs[Random.Range(0, npcPrefabs.Count)], spawnPoint.position, spawnPoint.rotation);
                NPCScript npc = obj.GetComponent<NPCScript>();
                npc.assignedRoute = assignedRoute;
                npc.startRoute = startRoute;
                npc.spawner = this;
                // atribumos a homeBase pelo cdigo porque os colegas spawnados nos elevadores precisam de voltar a casa
                // e como h sempre mais do que um elevador ele  atribudo quando  spawnado
                // para as rececnionistas como elas j existem no h problema porque no passam por este cdigo
                npc.homeBase = spawnPoint;
            }
        }
    }

    // chamado pelo NPCScript.OnDestroy quando o NPC  destrudo
    // permite ao spawner saber que pode spawnar mais um
    public void OnNPCDestroyed() {
        currentActive--;
    }
}