using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentDay = 1;
    public int currentFloor = 0;
    public const int TotalDays = 5;

    // pisos desbloqueados por ndice (0=receo, 1=executivo, 2=servidores, 3=sutes, 4=CEO)
    private bool[] floorUnlocked = new bool[5];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

    }

    void Start()
    {
        // rece��o e andar executivo acess�veis desde o in�cio
        UnlockFloor(0);
        UnlockFloor(1);

        GameEvent.OnDayEnded += HandleDayEnd;
        GameEvent.OnGameOver += HandleGameOver;
    }

    void OnDestroy()
    {
        GameEvent.OnDayEnded -= HandleDayEnd;
        GameEvent.OnGameOver -= HandleGameOver;
    }

    public void SetCurrentFloor(int floorNumber)
    {
        currentFloor = floorNumber;
        NPCManager.Instance.SetActiveFloor(floorNumber);
        Debug.Log($"[GameManager] Jogador moveu-se para F{floorNumber}.");
    }

    private void HandleDayEnd()
    {
        SaveProgress();

        if (currentDay >= TotalDays)
        {
            // o jogador chegou ao fim � vai para o ecr� de escolha de final
            // (l�gica de final a implementar quando o sistema de intel estiver pronto)
            Debug.Log("[GameManager] �ltimo dia conclu�do.");
            return;
        }

        currentDay++;
        GameEvent.DayChanged(currentDay);
        Debug.Log($"[GameManager] Dia {currentDay} come�a.");
    }

    private void HandleGameOver()
    {
        Debug.Log("[GameManager] Game Over.");
        // carregar cena de game over
    }

    public void UnlockFloor(int index)
    {
        if (index < 0 || index >= floorUnlocked.Length) return;
        floorUnlocked[index] = true;
        Debug.Log($"[GameManager] Piso {index} desbloqueado.");
    }

    public bool IsFloorUnlocked(int index)
    {
        if (index < 0 || index >= floorUnlocked.Length) return false;
        return floorUnlocked[index];
    }

    private void SaveProgress()
    {
        // integrar com Unity SaveSystem ou PlayerPrefs quando necess�rio
        Debug.Log($"[GameManager] Progresso guardado (dia {currentDay}).");
    }

    // --- getters e setters para o SaveManager ---
    public bool[] GetFloorsUnlocked() { return (bool[])floorUnlocked.Clone(); }

    public void SetFloorsUnlocked(bool[] floors)
    {
        if (floors == null || floors.Length != floorUnlocked.Length) return;
        System.Array.Copy(floors, floorUnlocked, floorUnlocked.Length);
    }
}