using System.Collections.Generic;
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
        // receção, andar executivo e suítes acessíveis desde o início
        UnlockFloor(0);
        UnlockFloor(1);
        UnlockFloor(3); // suítes (floorNumber 4 → índice 3)

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
        // guarda nula: se o NPCManager ainda não existir (ou não suportar este índice)
        // a coroutine DoTravel não morre a meio e o teleporte/fecho do UI acontece na mesma
        if (NPCManager.Instance != null)
            NPCManager.Instance.SetActiveFloor(floorNumber);
        Debug.Log($"[GameManager] Jogador moveu-se para F{floorNumber}.");
    }

    private void HandleDayEnd()
    {
        SaveProgress();

        if (currentDay >= TotalDays)
        {
            // o jogador chegou ao fim — vai para o ecrã de escolha de final
            // (lógica de final a implementar quando o sistema de intel estiver pronto)
            Debug.Log("[GameManager] Último dia concluído.");
            return;
        }

        currentDay++;
        GameEvent.DayChanged(currentDay);
        Debug.Log($"[GameManager] Dia {currentDay} começa.");
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
        // integrar com Unity SaveSystem ou PlayerPrefs quando necessário
        Debug.Log($"[GameManager] Progresso guardado (dia {currentDay}).");
    }

    // --- sistema de contribuições para finais ---
    // índice 0 = Denúncia, 1 = Extorsão, 2 = Lealdade
    private List<string>[] endingContributions = new List<string>[3] {
        new List<string>(), new List<string>(), new List<string>()
    };

    public void RegisterEndingContribution(int endingIndex, string sourceID) {
        if (endingIndex < 0 || endingIndex >= 3) return;
        if (!endingContributions[endingIndex].Contains(sourceID)) {
            endingContributions[endingIndex].Add(sourceID);
            Debug.Log($"[GameManager] Contribuição para final {endingIndex}: {sourceID}. Total: {endingContributions[endingIndex].Count}");
        }
    }

    public int GetEndingContributionCount(int endingIndex) {
        if (endingIndex < 0 || endingIndex >= 3) return 0;
        return endingContributions[endingIndex].Count;
    }

    // --- getters e setters para o SaveManager ---
    public bool[] GetFloorsUnlocked() { return (bool[])floorUnlocked.Clone(); }

    public void SetFloorsUnlocked(bool[] floors)
    {
        if (floors == null || floors.Length != floorUnlocked.Length) return;
        System.Array.Copy(floors, floorUnlocked, floorUnlocked.Length);
    }
}