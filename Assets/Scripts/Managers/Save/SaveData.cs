using System;
using System.Collections.Generic;

// classe que guarda todo o estado do jogo para ser serializada em JSON
// todos os campos são públicos para o JsonUtility conseguir ler/escrever
[Serializable]
public class SaveData {
    // dia e tempo
    public int currentDay;
    public float currentMinutes;
    public float accumulatedSleep;
    public int coffeesTaken;

    // posição do jogador
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerRotY;

    // stats do jogador (FOR, PER, RES, CAR, INT, AGI, SOR)
    public int[] playerStats;

    // piso atual e pisos desbloqueados
    public int currentFloor;
    public bool[] floorsUnlocked;

    // suspeita
    public float suspicionValue;

    // lanterna
    public float flashlightBattery;
    public bool hasFlashlight;

    // inventário de intel (nomes dos ScriptableObjects)
    public List<string> collectedIntelNames;

    // documentos -> escolhas do jogador
    public List<string> documentChoiceKeys;
    public List<string> documentChoiceValues;

    // company awareness
    public float companyAwareness;

    // dayManager
    public bool finalObjectiveCompleted;

    // camaras desbloqueadas
    public bool[] cameraUnlocked;
    public int hackLevel;
}