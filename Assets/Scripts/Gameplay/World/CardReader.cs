using System.Collections;
using UnityEngine;

public class CardReader : InteractableObject {

    public string cardID;
    public string cardName;

    private string lockedTooltip = "E para usar cartão";
    private string unlockedTooltip = "Acesso Autorizado";

    public Renderer statusLedRenderer;
    public Material ledLockedMaterial;
    public Material ledUnlockedMaterial;
    public Material ledOffMaterial;

    [HideInInspector] public bool isUnlocked = false;
    private bool isProcessing = false;

    protected override void Awake() {
        base.Awake();
        objectName = "Leitor de Cartões";

        tooltipMessage = isUnlocked ? unlockedTooltip : lockedTooltip;
        UpdateLedColor(isUnlocked ? ledUnlockedMaterial : ledLockedMaterial);
    }

    // comunica com o PlayerController para validar se o jogador tem a referência certa da credencial no inventário invisível dele
    public override void Interact() {
        if (isUnlocked) {
            Debug.Log($"[{gameObject.name}] Acesso já está autorizado.");
            return;
        }

        if (isProcessing) return;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsCurrentStepGate("tut_card")) {
            TutorialManager.Instance.CompleteCurrentStep();
        }

        if (PlayerController.Instance.HasCardCredential(cardID)) {
            StartCoroutine(ProcessUnlockRoutine());
        } else {
            StartCoroutine(ProcessAccessDeniedRoutine());
        }
    }

    // mal o gajo tem a credencial, espetamos o material verde no LED e chamamos o SoundManager para dar aquele feedback auditivo clássico de sucesso
    private IEnumerator ProcessUnlockRoutine() {
        isProcessing = true;
        isUnlocked = true;
        tooltipMessage = unlockedTooltip;

        Debug.Log($"[{gameObject.name}] Acesso AUTORIZADO com cartão: {cardName}");

        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.cardReaderSuccess);
        UpdateLedColor(ledUnlockedMaterial);

        yield return new WaitForSeconds(0.5f);

        isProcessing = false;
    }

    // caso dê erro, o SoundManager chuta um beep chato e o leitor pisca em vermelho para o gajo perceber logo que ainda tem que explorar mais o nível
    private IEnumerator ProcessAccessDeniedRoutine() {
        isProcessing = true;

        string cardNeeded = cardName;
        Debug.Log($"[{gameObject.name}] Acesso NEGADO! Falta credencial: {cardNeeded}");

        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.buzzerWrong2);

        string originalTooltip = tooltipMessage;
        tooltipMessage = $"Necessita de {cardNeeded}";
        UIManager.Instance.ShowTooltip(tooltipMessage);

        for (int i = 0; i < 3; i++) {
            UpdateLedColor(ledLockedMaterial);
            yield return new WaitForSeconds(0.15f);
            UpdateLedColor(ledOffMaterial);
            yield return new WaitForSeconds(0.15f);
        }

        UpdateLedColor(ledLockedMaterial);
        tooltipMessage = originalTooltip;

        if (CameraScript.Instance.currentTarget == this) {
            UIManager.Instance.ShowTooltip(tooltipMessage);
        }

        isProcessing = false;
    }

    private void UpdateLedColor(Material mat) {
        statusLedRenderer.material = mat;
    }
}
