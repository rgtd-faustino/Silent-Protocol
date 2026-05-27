using UnityEngine;

/// <summary>
/// Coloca este script no GameObject do telefone de secretária.
/// Precisa de um Collider (Is Trigger) para detectar o jogador.
/// Arrasta até 3 PhoneCallData no Inspector — cada um é um canal independente.
/// </summary>
public class PhoneInterceptScript : MonoBehaviour
{
    [Header("Canais disponíveis neste telefone (máx 3)")]
    [SerializeField] private PhoneCallData[] callChannels;

    [Header("Suspeita instantânea ao interceptar")]
    [Tooltip("Valor entre 0 e 1 adicionado de forma one-shot quando o jogador inicia a intercepção.")]
    private float suspicionOnUse = 0.04f;

    [Header("A chamada só está activa nestas horas do jogo")]
    private float activeFromHour = 9f;
    private float activeUntilHour = 17.5f;

    private bool playerNearby = false;

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Update()
    {
        if (!playerNearby) return;
        if (PhoneCallUI.IsOpen) return;

        float hora = TimeManager.Instance.GetCurrentTimeInHours();
        bool  horarioValido = hora >= activeFromHour && hora <= activeUntilHour;

        if (!horarioValido)
        {
            UIManager.Instance.ShowTooltip("Sem chamadas activas");
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SuspicionManager.Instance.AddInstantSuspicion(suspicionOnUse);
            PhoneCallUI.Instance.OpenCall(callChannels);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        UIManager.Instance.ShowTooltip("E  — Interceptar chamada");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        UIManager.Instance.HideTooltip();
    }
}
