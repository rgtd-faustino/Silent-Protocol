using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour {

    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private GameObject npcPrefab1;
    [SerializeField] private GameObject npcPrefab2;
    [SerializeField] private GameObject npcPrefab3;

    // ponto de spawn na cena (porta de entrada, receção, etc.)
    private Transform spawnPoint;

    // rota fixa atribuída a cada NPC spawnado por este spawner
    // ex: CAMINHO1 para colegas, DEAMBULAR para visitantes
    // se null, o NPC usa o sistema aleatório normal do NPCManager
    [SerializeField] private PatrolRoute assignedRoute;

    [SerializeField] private PatrolRoute startRoute;

    // quantos NPCs deste spawner podem existir na cena ao mesmo tempo
    [SerializeField] private int maxActive = 3;

    // segundos entre cada tentativa de spawn
    [SerializeField] private float spawnInterval = 30f;

    // contador interno — sobe no spawn, desce quando um NPC é destruído via OnNPCDestroyed
    private int currentActive = 0;


    void Start() {
        spawnPoint = transform;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine() {
        while (true) {
            yield return new WaitForSeconds(TimeManager.Instance.ToRealSeconds(spawnInterval));

            if (currentActive < maxActive) {
                // filtra os prefabs que estão preenchidos e escolhe um random
                List<GameObject> available = new List<GameObject>();

                if (npcPrefab != null) 
                    available.Add(npcPrefab);
                if (npcPrefab1 != null) 
                    available.Add(npcPrefab1);
                if (npcPrefab2 != null) 
                    available.Add(npcPrefab2);

                currentActive++;
                GameObject obj = Instantiate(available[Random.Range(0, available.Count)], spawnPoint.position, spawnPoint.rotation);
                NPCScript npc = obj.GetComponent<NPCScript>();
                npc.assignedRoute = assignedRoute;
                npc.startRoute = startRoute;
                npc.spawner = this;
                // atribuímos a homeBase pelo código porque os colegas spawnados nos elevadores precisam de voltar a casa
                // e como há sempre mais do que um elevador ele é atribuído quando é spawnado
                // para as rececnionistas como elas já existem não há problema porque não passam por este código
                npc.homeBase = spawnPoint;
            }
        }
    }

    // chamado pelo NPCScript.OnDestroy quando o NPC é destruído
    // permite ao spawner saber que pode spawnar mais um
    public void OnNPCDestroyed() {
        currentActive--;
    }
}