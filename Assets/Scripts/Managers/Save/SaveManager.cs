using System.Collections.Generic;
using System.IO;
using UnityEngine;

// responsável por gravar e carregar o ficheiro de save
// usa JSON simples (JsonUtility) e guarda em Application.persistentDataPath
public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // verifica se já existe um ficheiro de save no disco
    public bool HasSave() {
        return File.Exists(SavePath);
    }

    // apaga o ficheiro de save (usado no New Game e no Abandon)
    public void DeleteSave() {
        if (File.Exists(SavePath)) {
            File.Delete(SavePath);
            Debug.Log("[SaveManager] Save apagado.");
        }
    }

    // recolhe o estado de todos os managers e grava em JSON
    public void Save() {
        SaveData data = new SaveData();

        // dia e tempo
        data.currentDay = GameManager.Instance.currentDay;
        data.currentMinutes = TimeManager.Instance.GetCurrentMinutes();
        data.accumulatedSleep = TimeManager.Instance.GetAccumulatedSleep();
        data.coffeesTaken = TimeManager.Instance.GetCoffeesTaken();

        // posição do jogador
        Transform player = PlayerController.Instance.transform;
        data.playerPosX = player.position.x;
        data.playerPosY = player.position.y;
        data.playerPosZ = player.position.z;
        data.playerRotY = player.eulerAngles.y;

        // stats
        data.playerStats = (int[])PlayerStats.Instance.Stats.Clone();

        // piso
        data.currentFloor = GameManager.Instance.currentFloor;
        data.floorsUnlocked = GameManager.Instance.GetFloorsUnlocked();

        // suspeita
        data.suspicionValue = SuspicionManager.Instance.GetSuspicionRatio();

        // lanterna
        data.flashlightBattery = FlashlightController.Instance.GetBatteryRatio();
        data.hasFlashlight = PlayerController.Instance.hasFlashlight;

        // intel inventário
        data.collectedIntelNames = IntelInventory.Instance.GetCollectedIntelNames();

        // documentos
        Dictionary<string, string> choices = DocumentManager.Instance.GetAllChoices();
        data.documentChoiceKeys = new List<string>();
        data.documentChoiceValues = new List<string>();
        foreach (var pair in choices) {
            data.documentChoiceKeys.Add(pair.Key);
            data.documentChoiceValues.Add(pair.Value);
        }

        // company awareness
        data.companyAwareness = DocumentManager.Instance.GetCompanyAwareness();

        // cameras
        data.cameraUnlocked = CameraSystem.Instance.cameraUnlocked;
        data.hackLevel = CameraHackPuzzle.HackLevel;

        // gravar
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("[SaveManager] Jogo guardado em: " + SavePath);
    }

    // lê o ficheiro JSON e devolve o SaveData (ou null se não existir)
    public SaveData Load() {
        if (!HasSave()) {
            Debug.LogWarning("[SaveManager] Nenhum save encontrado.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("[SaveManager] Save carregado.");
        return data;
    }

    // aplica o SaveData a todos os managers do jogo
    public void ApplySave(SaveData data) {
        if (data == null) return;

        // dia
        GameManager.Instance.SetCurrentDay(data.currentDay);

        // tempo
        TimeManager.Instance.SetCurrentMinutes(data.currentMinutes);
        TimeManager.Instance.SetAccumulatedSleep(data.accumulatedSleep);
        TimeManager.Instance.SetCoffeesTaken(data.coffeesTaken);

        // posição do jogador
        CharacterController cc = PlayerController.Instance.GetComponent<CharacterController>();
        // o CharacterController bloqueia transform.position, temos de o desativar brevemente
        cc.enabled = false;
        PlayerController.Instance.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
        PlayerController.Instance.transform.eulerAngles = new Vector3(0f, data.playerRotY, 0f);
        cc.enabled = true;

        // stats
        PlayerStats.Instance.SetStats(data.playerStats);

        // piso
        GameManager.Instance.currentFloor = data.currentFloor;
        GameManager.Instance.SetFloorsUnlocked(data.floorsUnlocked);

        // suspeita
        SuspicionManager.Instance.SetSuspicionDirect(data.suspicionValue);

        // lanterna
        FlashlightController.Instance.SetBatteryRatio(data.flashlightBattery);
        PlayerController.Instance.hasFlashlight = data.hasFlashlight;

        // intel
        IntelInventory.Instance.RestoreIntelFromNames(data.collectedIntelNames);

        // documentos
        if (data.documentChoiceKeys != null) {
            for (int i = 0; i < data.documentChoiceKeys.Count; i++)
                DocumentManager.Instance.SaveChoice(data.documentChoiceKeys[i], data.documentChoiceValues[i]);
        }

        // company awareness
        DocumentManager.Instance.SetCompanyAwareness(data.companyAwareness);

        // cameras
        if (data.cameraUnlocked != null)
            CameraSystem.Instance.SetCameraUnlocked(data.cameraUnlocked);
        CameraHackPuzzle.HackLevel = data.hackLevel;

        Debug.Log("[SaveManager] Save aplicado com sucesso.");
    }
}