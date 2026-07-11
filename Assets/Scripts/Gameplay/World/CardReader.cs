using System.Collections;
using UnityEngine;
using static SuspicionManager;

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

        // mostra a tooltip suposta de acordo com se começa bloqueado ou desbloqueado
        tooltipMessage = isUnlocked ? unlockedTooltip : lockedTooltip;
        UpdateLedColor(isUnlocked ? ledUnlockedMaterial : ledLockedMaterial); // incializa também a cor correta
    }

    // comunica com o PlayerController para validar se o jogador tem a referência certa da credencial no inventário dele
    public override void Interact() {
        if (isUnlocked) {
            Debug.Log($"[{gameObject.name}] Acesso já está autorizado.");
            return;
        }

        if (isProcessing) 
            return;

        // caso o tutorial exija que o jogador apanhe um cartão para poder seguir em frente com o mesmo (atualmente não é obrigado)
        if (TutorialManager.Instance.IsCurrentStepGate("tut_card")) {
            TutorialManager.Instance.CompleteCurrentStep();
        }

        if (PlayerController.Instance.HasCardCredential(cardID)) {
            StartCoroutine(ProcessUnlockRoutine());
        } else {
            StartCoroutine(ProcessAccessDeniedRoutine());
        }
    }

    // mal o jogador tem a credencial, metemos o material verde no LED e chamamos o SoundManager para dar o feedback auditivo de sucesso
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

    // caso dê erro, o SoundManager faz um beep mau e o leitor pisca em vermelho para o jogador perceber logo que ainda tem que explorar mais
    private IEnumerator ProcessAccessDeniedRoutine() {
        isProcessing = true;

        string cardNeeded = cardName;
        Debug.Log($"[{gameObject.name}] Acesso NEGADO! Falta credencial: {cardNeeded}");

        SuspicionManager.Instance.IncreaseSuspicion(1.5f, GetInstanceID(), SuspicionSource.CardCodeDenied); // sobe a suspeita porque o jogador tentou aceder a algo que não devia
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
