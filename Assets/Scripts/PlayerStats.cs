using UnityEngine;

// guarda os 7 atributos da personagem definidos na criação
public class PlayerStats : MonoBehaviour {
    public static PlayerStats Instance;

    [Header("Atributos (distribuídos na criação do personagem)")]
    [Range(1, 10)] public int strength = 5; // capacidade física
    [Range(1, 10)] public int perception = 5; // observação e atenção ao detalhe
    [Range(1, 10)] public int endurance = 5; // resistência à fadiga
    [Range(1, 10)] public int charisma = 5; // opções sociais e de diálogo
    [Range(1, 10)] public int intellect = 5; // eficácia nos puzzles
    [Range(1, 10)] public int agility = 5; // velocidade e furtividade
    [Range(1, 10)] public int luck = 5; // fatores aleatórios a favor

    // pontos totais disponíveis para distribuir na criação (a implementar no menu)
    public const int PointPool = 35;

    void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

    }

    // exemplos de como outros sistemas devem usar os atributos:
    // - SleepStage.threshold pode ser modificado por endurance
    // - NPCScript.fovRange do jogador pode ser aumentado por perception
    // - puzzle success chance pode ser influenciada por intellect
    // - noise radius pode ser reduzido por agility
}