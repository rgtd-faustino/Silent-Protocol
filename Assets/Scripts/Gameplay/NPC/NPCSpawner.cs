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

    // se associarmos uma rota especifica ignoramos o algoritmo do NPCManager e forcamos o NPC a este caminho
    [SerializeField] private PatrolRoute assignedRoute;

    // antes do NPC começar a fazer uma rota qualquer irá percorrer esta primeiro
    [SerializeField] private PatrolRoute startRoute;

    // limita o número de instances que o spawner pode criar
    [SerializeField] private int maxActive = 3;

    [SerializeField] private float spawnInterval = 30f; // intervalo de tempo que vai instanciando NPC

    // quantos NPC foram já instanciados
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
                // a homebase representa o elevador a que o NPC poderá voltar no fim da sua rota, caso aconteça
                npc.homeBase = spawnPoint;
            }
        }
    }

    // apanha o callback do recycle do NPCScript para decrementar o contador
    public void OnNPCDestroyed() {
        currentActive--;
    }
}