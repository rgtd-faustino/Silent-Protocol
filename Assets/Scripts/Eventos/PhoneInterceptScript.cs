using UnityEngine;

public class PhoneInterceptScript : InteractableObject {
    [Header("Canais disponíveis neste telefone (máx 3)")]
    [SerializeField] private PhoneCallData[] callChannels;

    [Header("Suspeita instantânea ao interceptar")]
    private float suspicionOnUse = 0.04f;

    [Header("Horário activo")]
    private float activeFromHour = 9f;
    private float activeUntilHour = 17.5f;

    protected override void Awake() {
        base.Awake();
        objectName = "Telefone";
        tooltipMessage = "E — Intercetar chamada";
    }

    public override void Interact() {
        if (PhoneCallUI.IsOpen) return;

        float hora = TimeManager.Instance.GetCurrentTimeInHours();
        if (hora < activeFromHour || hora > activeUntilHour) {
            Debug.Log("[Telefone] Sem chamadas activas neste horário.");
            return;
        }

        SuspicionManager.Instance.AddInstantSuspicion(suspicionOnUse);
        PhoneCallUI.Instance.OpenCall(callChannels);
    }
}