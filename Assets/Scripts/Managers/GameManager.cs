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

    [Header("Final do Jogo")]
    [Tooltip("Percentagem mínima (0-100) para o jogador atingir o final bom.")]
    [SerializeField] private float endingThreshold = 50f;
    [Tooltip("Tecla de teste (só ativa no Editor) para forçar o final do relatório sem esperar pelo último dia.")]
    [SerializeField] private KeyCode debugEndingKey = KeyCode.Z;

    private bool endingTriggered = false;

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
        UnlockFloor(3); // suítes (floorNumber 4 -> índice 3)

        GameEvent.OnDayEnded += HandleDayEnd;
        GameEvent.OnGameOver += HandleGameOver;
        GameEvent.OnPlayerExhausted += HandleExhaustion;
    }

    void OnDestroy()
    {
        GameEvent.OnDayEnded -= HandleDayEnd;
        GameEvent.OnGameOver -= HandleGameOver;
        GameEvent.OnPlayerExhausted -= HandleExhaustion;
    }

#if UNITY_EDITOR
    void Update()
    {
        // tecla de teste, só existe em builds do Editor: força o final do relatório sem precisar de chegar ao último dia
        if (Input.GetKeyDown(debugEndingKey))
            TriggerReportEnding();
    }
#endif

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
            // o jogador chegou ao fim — decide o final consoante a percentagem de intel recolhida
            Debug.Log("[GameManager] Último dia concluído.");
            TriggerReportEnding();
            return;
        }

        currentDay++;
        GameEvent.DayChanged(currentDay);
        Debug.Log($"[GameManager] Dia {currentDay} começa.");
    }

    /// <summary>
    /// Chamado quando o jogador envia o relatório final — pelo botão "Enviar Relatório" no email,
    /// ou automaticamente no fim do último dia. Decide o final consoante a percentagem de intel
    /// recolhida: ending 1 = final bom (percentagem >= endingThreshold), ending 2 = final mau.
    /// </summary>
    public void TriggerReportEnding()
    {
        if (endingTriggered) return;

        float percentagem = IntelInventory.Instance.GetTotalPercentage();
        int ending = percentagem >= endingThreshold ? 1 : 2;

        Debug.Log($"[GameManager] Relatório enviado — {percentagem:F0}% (limite {endingThreshold}%) -> ending {ending}.");
        FireEnding(ending);
    }

    /// <summary>
    /// Chamado quando a suspeita atinge o máximo (SuspicionManager -> GameEvent.OnGameOver).
    /// O jogador foi apanhado antes de conseguir agir.
    /// </summary>
    private void HandleGameOver()
    {
        if (endingTriggered) return;

        Debug.Log("[GameManager] Apanhado — suspeita atingiu o máximo.");
        // toca o som de morte ao ser apanhado
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.die);
        FireEnding(3);
    }

    /// <summary>
    /// Chamado quando o sono acumulado atinge o estágio severo (TimeManager -> GameEvent.OnPlayerExhausted).
    /// O jogador deixou-se ficar sem dormir tempo demais.
    /// </summary>
    private void HandleExhaustion()
    {
        if (endingTriggered) return;

        Debug.Log("[GameManager] Exaustão — sono acumulado atingiu o estágio severo.");
        FireEnding(4);
    }

    /// <summary>
    /// Ponto único de disparo do final: garante que só um final acontece por partida,
    /// independentemente de qual das três causas o acionou primeiro.
    /// </summary>
    private void FireEnding(int ending)
    {
        if (endingTriggered) return;
        endingTriggered = true;
        GameEvent.EndingReached(ending);
    }

    public void UnlockFloor(int index)
    {
        if (index < 0 || index >= floorUnlocked.Length) return;
        floorUnlocked[index] = true;
        Debug.Log($"[GameManager] Piso {index} desbloqueado.");
    }

    /// <summary>
    /// Repõe currentDay, currentFloor, pisos desbloqueados, contribuições para finais e a
    /// flag de final já disparado, para que um "Novo Jogo" comece mesmo do zero.
    /// </summary>
    public void ResetForNewGame()
    {
        currentDay = 1;
        currentFloor = 0;
        endingTriggered = false;

        for (int i = 0; i < floorUnlocked.Length; i++)
            floorUnlocked[i] = false;
        UnlockFloor(0);
        UnlockFloor(1);
        UnlockFloor(3);

        for (int i = 0; i < endingContributions.Length; i++)
            endingContributions[i].Clear();

        Debug.Log("[GameManager] Estado reiniciado para um novo jogo.");
    }

    public bool IsFloorUnlocked(int index)
    {
        if (index < 0 || index >= floorUnlocked.Length) return false;
        return floorUnlocked[index];
    }

    private void SaveProgress()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save();

        Debug.Log($"[GameManager] Progresso guardado (dia {currentDay}).");
    }

    // sistema de contribuições para finais
    // índice 0 = Denúncia, 1 = Extorsão, 2 = Lealdade
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

    // getters e setters para o SaveManager
    public bool[] GetFloorsUnlocked() { return (bool[])floorUnlocked.Clone(); }

    public void SetFloorsUnlocked(bool[] floors)
    {
        if (floors == null || floors.Length != floorUnlocked.Length) return;
        System.Array.Copy(floors, floorUnlocked, floorUnlocked.Length);
    }
}