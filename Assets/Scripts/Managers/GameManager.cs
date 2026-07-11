using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public int currentDay = 1;
    public int currentFloor = 0;
    public const int TotalDays = 5;

    // pisos desbloqueados por índice (0=receção, 1=executivo, 2=servidores, 3=suítes, 4=CEO)
    private bool[] floorUnlocked = new bool[5];

    [Header("Final do Jogo")]
    [SerializeField] private float endingThreshold = 50f; // percentagem mínima para o jogador atingir o final bom

    private bool endingTriggered = false;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        // receção, andar executivo e suítes acessíveis desde o início
        UnlockFloor(0);
        UnlockFloor(1);
        UnlockFloor(3); 

        GameEvent.OnDayEnded += HandleDayEnd;
        GameEvent.OnGameOver += HandleGameOver;
        GameEvent.OnGameOver += HandleExhaustion;

        // dispara o evento de início de dia e arranca o tutorial no dia 1
        // usamos uma corrotina para esperar o fim deste frame, para garantir que os outros managers já fizeram
        // o Awake/Start e podem apanhar o evento DayStarted sem problemas de concorrência de inicialização
        StartCoroutine(ShowTitleNextFrame());
    }

    void OnDestroy() {
        GameEvent.OnDayEnded -= HandleDayEnd;
        GameEvent.OnGameOver -= HandleGameOver;
        GameEvent.OnGameOver += HandleExhaustion;
    }

    private IEnumerator ShowTitleNextFrame() {
        yield return null;
        NotifyDayStarted();

        if (currentDay == 1) {
            // só corremos o tutorial no primeiro dia porque é o primeiro "nível" do jogo
            TutorialManager.Instance.StartTutorial();
        }
    }

    public void SetCurrentFloor(int floorNumber) {
        currentFloor = floorNumber;
        // para desligar a navegação e a física dos NPC que ficaram noutros andares
        NPCManager.Instance.SetActiveFloor(floorNumber);
    }

    // dispara GameEvent.DayStarted
    // chamado pelo TimeManager às 08:00 ou logo após o jogador dormir
    // fica centralizado aqui (em vez do TimeManager disparar o evento diretamente) para garantir que nunca dispara depois do jogo já ter terminado
    // por ex se o jogador adormecer exatamente na última noite
    public void NotifyDayStarted() {
        if (endingTriggered) 
           return;

        GameEvent.DayStarted(currentDay);
    }

    // ponto único que reage ao fim do dia (disparado via GameEvent.DayEnded() pelo TimeManager).
    private void HandleDayEnd() {
        SaveProgress();

        if (currentDay >= TotalDays) {
            // o jogador chegou ao fim, decide o final consoante a percentagem de intel recolhida
            Debug.Log("[GameManager] Último dia concluído.");
            TriggerReportEnding();
            return;
        }

        currentDay++;
        GameEvent.DayChanged(currentDay);
        // DayStarted NÃO dispara aqui — dispara às 08:00 via NotifyDayStarted() chamado pelo
        // TimeManager, ou logo após o jogador dormir. Preserva a distinção original entre
        // "o dia civil mudou" (meia-noite, DayChanged) e "o expediente começou" (08:00 / acordar, DayStarted).
        Debug.Log($"[GameManager] Dia {currentDay} começa.");
    }

    // chamado quando o jogador envia o relatório final pelo botão "Enviar Relatório" no email,
    // ou automaticamente no fim do último dia, depois decide o final consoante a percentagem de intel
    public void TriggerReportEnding() {
        if (endingTriggered) return;

        float percentagem = IntelInventory.Instance.GetTotalPercentage();
        int ending = percentagem >= endingThreshold ? 1 : 2;

        Debug.Log($"[GameManager] Relatório enviado — {percentagem:F0}% (limite {endingThreshold}%) -> ending {ending}.");
        FireEnding(ending);
    }

    // chamado quando a suspeita atinge o máximo (SuspicionManager -> GameEvent.OnGameOver)
    // o jogador foi apanhado antes de conseguir agir
    private void HandleGameOver() {
        if (endingTriggered) return;

        Debug.Log("[GameManager] Apanhado — suspeita atingiu o máximo.");
        // toca o som de morte ao ser apanhado
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2D, SoundManager.Instance.die);
        FireEnding(3);
    }

    // chamado quando o sono acumulado atinge o estágio severo (TimeManager -> GameEvent.OnPlayerExhausted)
    // o jogador deixou-se ficar sem dormir tempo demais
    private void HandleExhaustion() {
        if (endingTriggered) return;

        Debug.Log("[GameManager] Exaustão — sono acumulado atingiu o estágio severo.");
        FireEnding(4);
    }

    // ponto único de disparo do final: garante que só um final acontece por partida, independentemente de qual das três causas o acionou primeiro
    private void FireEnding(int ending) {
        if (endingTriggered) 
            return;

        endingTriggered = true;
        GameEvent.EndingReached(ending);
    }

    public void UnlockFloor(int index) {
        if (index < 0 || index >= floorUnlocked.Length) return;
        floorUnlocked[index] = true;
        Debug.Log($"[GameManager] Piso {index} desbloqueado.");
    }

    // repõe currentDay, currentFloor, pisos desbloqueados, contribuições para finais e a flag de final já disparado para que um "Novo Jogo" comece do zero
    public void ResetForNewGame() {
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

    public bool IsFloorUnlocked(int index) {
        if (index < 0 || index >= floorUnlocked.Length) 
            return false;

        return floorUnlocked[index];
    }

    private void SaveProgress() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save();

        Debug.Log($"[GameManager] Progresso guardado (dia {currentDay}).");
    }

    // sistema de contribuições para finais
    // índice 0 = Denúncia, 1 = Extorsão, 2 = Lealdade
    private List<string>[] endingContributions = new List<string>[3] {
        new List<string>(), new List<string>(), new List<string>()
    };

    public void RegisterEndingContribution(int endingIndex, string sourceID) {
        if (endingIndex < 0 || endingIndex >= 3) 
            return;

        if (!endingContributions[endingIndex].Contains(sourceID)) {
            endingContributions[endingIndex].Add(sourceID);
            Debug.Log($"[GameManager] Contribuição para final {endingIndex}: {sourceID}. Total: {endingContributions[endingIndex].Count}");
        }
    }

    // getters e setters para o SaveManager
    public void SetCurrentDay(int day) {
        currentDay = day;
    }
    public bool[] GetFloorsUnlocked() { return (bool[])floorUnlocked.Clone(); }
    public void SetFloorsUnlocked(bool[] floors) {
        if (floors == null || floors.Length != floorUnlocked.Length) return;
        System.Array.Copy(floors, floorUnlocked, floorUnlocked.Length);
    }
}