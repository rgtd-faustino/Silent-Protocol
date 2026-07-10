using UnityEngine;

public class PlayerStats : MonoBehaviour {
    public static PlayerStats Instance;

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

    // A UI de criação da personagem injeta aqui os pontos distribuídos no início da run. Fica alojado neste singleton para que qualquer classe como o PlayerController consiga consultar os atributos e mudar as mecânicas na hora.
    public void SetStats(int[] newStats) {
        if (newStats == null || newStats.Length != 7) 
            return;

        System.Array.Copy(newStats, Stats, 7);
    }
}