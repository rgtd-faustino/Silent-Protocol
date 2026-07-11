using System.Collections;
using UnityEngine;

public class PhoneInterceptScript : InteractableObject {
    [SerializeField] private PhoneCallData[] callChannels; // canais disponíveis neste telefone

    // aumenta logo a suspeita assim que o jogador atende, para impedir que fiquem a abusar dos telefones
    private float suspicionOnUse = 0.04f;

    // horário em que o telefonema ocorre e o jogador pode escutar
    public float activeFromHour = 9f;
    public float activeUntilHour = 17.5f;

    protected override void Awake() {
        base.Awake();
        objectName = "Telefone";

        // fazemos uma corrotina para não usarmos um update que seria muito ineficiente
        StartCoroutine(TooltipUpdateRoutine());
    }

    private IEnumerator TooltipUpdateRoutine() {
        while (true) {
            UpdateTooltip();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public override void Interact() {
        if (PhoneCallUI.IsOpen)
            return;

        float hora = TimeManager.Instance.GetCurrentTimeInHours();
        if (hora < activeFromHour || hora > activeUntilHour) {
            Debug.Log("[Telefone] Sem chamadas activas neste horário.");
            return;
        }

        SuspicionManager.Instance.AddInstantSuspicion(suspicionOnUse);
        PhoneCallUI.Instance.OpenCall(callChannels);
    }

    private void UpdateTooltip() {
        float hora = TimeManager.Instance.GetCurrentTimeInHours();

        if (hora >= activeFromHour && hora <= activeUntilHour) {
            tooltipMessage = "E - Intercetar chamada";
        } else {
            tooltipMessage = "Sem chamadas ativas atualmente";
        }
    }
}