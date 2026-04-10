using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int CurrentDay { get; private set; } = 1;
    public const int TotalDays = 5;

    // pisos desbloqueados por índice (0=receção, 1=executivo, 2=servidores, 3=suítes, 4=CEO)
    private bool[] floorUnlocked = new bool[5];

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

    }

    void Start() {
        // receção e andar executivo acessíveis desde o início
        UnlockFloor(0);
        UnlockFloor(1);

        GameEvent.OnDayEnded += HandleDayEnd;
        GameEvent.OnGameOver += HandleGameOver;
    }

    void OnDestroy() {
        GameEvent.OnDayEnded -= HandleDayEnd;
        GameEvent.OnGameOver -= HandleGameOver;
    }

    private void HandleDayEnd() {
        SaveProgress();

        if (CurrentDay >= TotalDays) {
            // o jogador chegou ao fim — vai para o ecrã de escolha de final
            // (lógica de final a implementar quando o sistema de intel estiver pronto)
            Debug.Log("[GameManager] Último dia concluído.");
            return;
        }

        CurrentDay++;
        GameEvent.DayChanged(CurrentDay);
        Debug.Log($"[GameManager] Dia {CurrentDay} começa.");
    }

    private void HandleGameOver() {
        Debug.Log("[GameManager] Game Over.");
        // carregar cena de game over
    }

    public void UnlockFloor(int index) {
        if (index < 0 || index >= floorUnlocked.Length) return;
        floorUnlocked[index] = true;
        Debug.Log($"[GameManager] Piso {index} desbloqueado.");
    }

    public bool IsFloorUnlocked(int index) {
        if (index < 0 || index >= floorUnlocked.Length) return false;
        return floorUnlocked[index];
    }

    private void SaveProgress() {
        // integrar com Unity SaveSystem ou PlayerPrefs quando necessário
        Debug.Log($"[GameManager] Progresso guardado (dia {CurrentDay}).");
    }
}