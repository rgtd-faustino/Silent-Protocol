using UnityEngine;

// guarda os 7 atributos da personagem definidos na criação
public class PlayerStats : MonoBehaviour {
    public static PlayerStats Instance;


    // Ordem: Força, Perceção, Resistência, Carisma, Intelecto, Agilidade, Sorte
    public int[] Stats = new int[7] { 1, 1, 1, 1, 1, 1, 1 };

    public int GetForca() {
        return Stats[0];
    }

    public int GetPercecao() {
        return Stats[1];
    }

    public int GetResistencia() {
        return Stats[2];
    }

    public int GetCarisma() {
        return Stats[3];
    }

    public int GetIntelecto() {
        return Stats[4];
    }

    public int GetAgilidade() {
        return Stats[5];
    }

    public int GetSorte() {
        return Stats[6];
    }


    void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    public void SetStats(int[] newStats) {
        if (newStats == null || newStats.Length != 7) 
            return;

        System.Array.Copy(newStats, Stats, 7);
    }


    // exemplos de como outros sistemas devem usar os atributos:
    // - SleepStage.threshold pode ser modificado por endurance
    // - NPCScript.fovRange do jogador pode ser aumentado por perception
    // - puzzle success chance pode ser influenciada por intellect
    // - noise radius pode ser reduzido por agility
}