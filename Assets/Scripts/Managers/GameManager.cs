using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentDay = 1;
    public int currentFloor = 0;
    public const int TotalDays = 5;

    // optámos por um array de booleanos indexado diretamente pelo número do piso, assim poupamos a complexidade de um dicionário, até porque são poucos andares
    private bool[] floorUnlocked = new bool[5];

    [Header("Final do Jogo")]
    [Tooltip("Percentagem mínima (0-100) para o jogador atingir o final bom.")]
    [SerializeField] private float endingThreshold = 50f;
    [Tooltip("Tecla de teste para forçar o final enquanto não há um gatilho real no jogo.")]
    [SerializeField] private KeyCode debugEndingKey = KeyCode.Z;

    private bool endingTriggered = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        UnlockFloor(0);
        UnlockFloor(1);
        UnlockFloor(3);

        GameEvent.OnDayEnded += HandleDayEnd;
        GameEvent.OnGameOver += HandleGameOver;
    }

    void OnDestroy()
    {
        GameEvent.OnDayEnded -= HandleDayEnd;
        GameEvent.OnGameOver -= HandleGameOver;
    }

    void Update()
    {
        if (Input.GetKeyDown(debugEndingKey))
            TriggerEnding();
    }

    // o NPCManager e o player dependem disto para saber se os cálculos de raycast fazem sentido para a posição atual, senão consumíamos recursos noutros pisos
    public void SetCurrentFloor(int floorNumber)
    {
        currentFloor = floorNumber;
        if (NPCManager.Instance != null)
            NPCManager.Instance.SetActiveFloor(floorNumber);
        Debug.Log($"[GameManager] Jogador moveu-se para F{floorNumber}.");
    }

    private void HandleDayEnd()
    {
        SaveProgress();

        if (currentDay >= TotalDays)
        {
            Debug.Log("[GameManager] Último dia concluído.");
            TriggerEnding();
            return;
        }

        currentDay++;
        GameEvent.DayChanged(currentDay);
        Debug.Log($"[GameManager] Dia {currentDay} começa.");
    }

    // esta lógica agrupa o esforço da semana inteira e avalia-o em relação ao limiar estipulado no inspector,
    // definindo qual o estado de narrativa a mandar para a UI final
    private void TriggerEnding()
    {
        if (endingTriggered) return;
        endingTriggered = true;

        float percentagem = IntelInventory.Instance.GetTotalPercentage();
        int ending = percentagem >= endingThreshold ? 1 : 2;

        Debug.Log($"[GameManager] Final acionado - {percentagem:F0}% (limite {endingThreshold}%) -> ending {ending}.");
        GameEvent.EndingReached(ending);
    }

    private void HandleGameOver()
    {
        Debug.Log("[GameManager] Game Over.");
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.die);
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
        Debug.Log($"[GameManager] Progresso guardado (dia {currentDay}).");
    }

    // listas separadas para cada fação final. guardamos o ID do source (como um documento específico) para o jogador não poder farmar a mesma decisão várias vezes
    private List<string>[] endingContributions = new List<string>[3] {
        new List<string>(), new List<string>(), new List<string>()
    };

    public void RegisterEndingContribution(int endingIndex, string sourceID)
    {
        if (endingIndex < 0 || endingIndex >= 3) return;
        if (!endingContributions[endingIndex].Contains(sourceID))
        {
            endingContributions[endingIndex].Add(sourceID);
            Debug.Log($"[GameManager] Contribuição para final {endingIndex}: {sourceID}. Total: {endingContributions[endingIndex].Count}");
        }
    }

    public int GetEndingContributionCount(int endingIndex)
    {
        if (endingIndex < 0 || endingIndex >= 3) return 0;
        return endingContributions[endingIndex].Count;
    }

    public bool[] GetFloorsUnlocked() { return (bool[])floorUnlocked.Clone(); }

    public void SetFloorsUnlocked(bool[] floors)
    {
        if (floors == null || floors.Length != floorUnlocked.Length) return;
        System.Array.Copy(floors, floorUnlocked, floorUnlocked.Length);
    }
}