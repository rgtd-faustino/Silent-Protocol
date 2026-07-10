using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour {
    [SerializeField] private List<GameObject> npcPrefabs = new List<GameObject>();

    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private GameObject npcPrefab1;
    [SerializeField] private GameObject npcPrefab2;
    [SerializeField] private GameObject npcPrefab3;
    
    private Transform spawnPoint;

    // Se associarmos uma rota especifica ignoramos o algoritmo do NPCManager e forcamos o NPC a este caminho. E incrivel para controlar loops rigorosos.
    [SerializeField] private PatrolRoute assignedRoute;

    [SerializeField] private PatrolRoute startRoute;

    // Limita o spawn de instanciacoes infinitas para travar derrapagens de performance no CPU.
    [SerializeField] private int maxActive = 3;

    [SerializeField] private float spawnInterval = 30f;

    // Rastreamos as entradas cruzando o OnNPCDestroyed da classe NPCScript. Assim repomos sempre a fauna na sala sem exceder a quota.
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
                // Injetamos a homeBase a bruto baseados neste transform. Isto soluciona aquelas paragens onde a malta nao sabia para que elevador devia regressar no final da jornada.
                npc.homeBase = spawnPoint;
            }
        }
    }

    // Apanha o callback do recycle para baixar o tracker de atividade e garantir que o iterador liberta mais espaco na memoria.
    public void OnNPCDestroyed() {
        currentActive--;
    }
}