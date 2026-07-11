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

    // a UI de criação da personagem injeta aqui os pontos distribuídos no início da run
    // fica alojado neste singleton para que qualquer classe como o PlayerController consiga consultar os atributos e mudar as mecânicas
    public void SetStats(int[] newStats) {
        if (newStats == null || newStats.Length != 7)
            return;

        // copiamos os valores para dentro do array Stats já existente em vez de substituir a referência (Stats = newStats)
        // para que qualquer script que já tenha guardado uma referência a Stats continue válido e veja os valores atualizados
        System.Array.Copy(newStats, Stats, 7);
    }
}