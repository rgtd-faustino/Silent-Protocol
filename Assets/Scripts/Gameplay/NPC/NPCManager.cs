using UnityEngine;

public class NPCManager : MonoBehaviour {
    public static NPCManager Instance;

    public Transform player;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {

    }

}