using System.Collections;
using UnityEngine;

public class CardReader : InteractableObject {

    public string cardID;
    public string cardName;


    private string lockedTooltip = "E para usar cartÃ£o";
    private string unlockedTooltip = "Acesso Autorizado";

    public Renderer statusLedRenderer;
    public Material ledLockedMaterial;
    public Material ledUnlockedMaterial;
    public Material ledOffMaterial;

    [HideInInspector] public bool isUnlocked = false;
    private bool isProcessing = false;

    protected override void Awake() {
        base.Awake();
        objectName = "Leitor de CartÃµes";

        // definimos a tooltip inicial consoante o estado de desbloqueio
        tooltipMessage = isUnlocked ? unlockedTooltip : lockedTooltip;

        // mudamos a cor do leitor de cartÃµes bom base no estado do mesmo
        UpdateLedColor(isUnlocked ? ledUnlockedMaterial : ledLockedMaterial);
    }

    public override void Interact() {
        if (isUnlocked) {
            Debug.Log($"[{gameObject.name}] Acesso jÃ¡ estÃ¡ autorizado.");
            return;
        }

        if (isProcessing) return;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_card")) {
            TutorialManager.Instance.CompleteCurrentStep();
        }

        // verificamos se o jogador possui a credencial com o ID correspondente
        if (PlayerController.Instance.HasCardCredential(cardID)) {
            StartCoroutine(ProcessUnlockRoutine());
        } else {
            StartCoroutine(ProcessAccessDeniedRoutine());
        }
    }

    private IEnumerator ProcessUnlockRoutine() {
        isProcessing = true;
        isUnlocked = true;
        tooltipMessage = unlockedTooltip;

        Debug.Log($"[{gameObject.name}] Acesso AUTORIZADO com cartÃ£o: {cardName}");

        // som de sucesso do leitor de cartÃµes
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.cardReaderSuccess);

        // feedback LED verde permanente porque o acesso ficou autorizado
        UpdateLedColor(ledUnlockedMaterial);

        yield return new WaitForSeconds(0.5f);

        isProcessing = false;
    }

    private IEnumerator ProcessAccessDeniedRoutine() {
        isProcessing = true;

        string cardNeeded = cardName;
        Debug.Log($"[{gameObject.name}] Acesso NEGADO! Falta credencial: {cardNeeded}");

        // som de erro do leitor de cartÃµes
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.buzzerWrong2);

        // alteramos temporariamente a mensagem do HUD para avisar o jogador do cartÃ£o que falta
        string originalTooltip = tooltipMessage;
        tooltipMessage = $"Necessita de {cardNeeded}";
        UIManager.Instance.ShowTooltip(tooltipMessage);

        // flash LED vermelho/apagado para simular alarme/erro eletrÃ³nico
        for (int i = 0; i < 3; i++) {
            UpdateLedColor(ledLockedMaterial);
            yield return new WaitForSeconds(0.15f);
            UpdateLedColor(ledOffMaterial);
            yield return new WaitForSeconds(0.15f);
        }

        // depois do aviso, voltamos a meter o LED e a tooltip original
        UpdateLedColor(ledLockedMaterial);
        tooltipMessage = originalTooltip;

        // se o jogador ainda estiver a olhar para o leitor de cartÃµes, repÃµe a tooltip
        if (CameraScript.Instance.currentTarget == this) {
            UIManager.Instance.ShowTooltip(tooltipMessage);
        }

        isProcessing = false;
    }

    private void UpdateLedColor(Material mat) {
        statusLedRenderer.material = mat;
    }


}
