using UnityEngine;

public class PCInteractable : InteractableObject
{
    public GameObject pcCanvas;
    
    // Controla se a interface do PC está visível ou não para evitar múltiplos inputs
    private bool isOpen = false;

    [Header("Segurança")]
    public bool precisaDeCartao = false;
    public string cardID;
    public string cardName;

    public bool precisaDePin = false;
    public string pinCorreto = "1234";

    // Regista se o jogador já validou as credenciais (cartão ou PIN)
    // Fica a true após o primeiro sucesso para não voltar a pedir na mesma sessão
    private bool isUnlocked = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Computador";
        tooltipMessage = "E para usar computador";
    }

    // Chamado pelo PlayerController via raycast de interação
    // Decide se abre o PC diretamente ou se reencaminha para o fluxo de segurança
    public override void Interact()
    {
        if (isOpen)
        {
            ClosePC();
            return;
        }

        if (isUnlocked || (!precisaDeCartao && !precisaDePin))
        {
            OpenPC();
            return;
        }

        TentarDesbloquear();
    }

    // Processa a autenticação em duas fases: primeiro valida o cartão silenciosamente e depois pede o PIN com UI
    private void TentarDesbloquear()
    {
        if (precisaDeCartao && !PlayerController.Instance.HasCardCredential(cardID))
        {
            StartCoroutine(AvisoCartaoEmFalta());
            return;
        }

        if (precisaDePin)
        {
            PinEntryUI.Instance.AbrirPin(
                pinCorreto,
                callbackCorreto: () =>
                {
                    isUnlocked = true;
                    OpenPC();
                },
                callbackCancelar: () => { }
            );
            return;
        }

        isUnlocked = true;
        OpenPC();
    }

    // Dá feedback visual temporário na tooltip quando o jogador não tem o cartão certo
    private System.Collections.IEnumerator AvisoCartaoEmFalta()
    {
        string original = tooltipMessage;
        tooltipMessage = $"Necessita de {cardName}";
        UIManager.Instance.ShowTooltip(tooltipMessage);

        yield return new WaitForSeconds(1.5f);

        tooltipMessage = original;
        if (CameraScript.Instance.currentTarget == this)
            UIManager.Instance.ShowTooltip(tooltipMessage);
    }

    // Bloqueia o movimento e a câmara do jogador e mostra a interface do PC
    // Aproveita para avisar os componentes do servidor (Server1UI e Server2UI) de que o PC foi reaberto
    private void OpenPC()
    {
        Debug.Log($"[PCInteractable] OpenPC chamado! pcCanvas = {pcCanvas.name}, estava ativo antes? {pcCanvas.activeSelf}");
        pcCanvas.SetActive(true);
        Debug.Log($"[PCInteractable] pcCanvas.activeSelf agora = {pcCanvas.activeSelf}, activeInHierarchy = {pcCanvas.activeInHierarchy}");
        pcCanvas.SetActive(true);
        PlayerController.Instance.canMoveRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.Instance.RefreshPCInterface();
        isOpen = true;

        Server1UI srv1 = pcCanvas.GetComponentInChildren<Server1UI>();
        if (srv1 != null) srv1.OnOpen();
        Server2UI srv2 = pcCanvas.GetComponentInChildren<Server2UI>();
        if (srv2 != null) srv2.OnOpen();
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.P))
            ClosePC();
    }

    // Oculta a interface e devolve o controlo ao PlayerController
    private void ClosePC()
    {
        pcCanvas.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isOpen = false;
    }

    // Ativa o brilho no modelo do PC se existirem tarefas no TaskManager que dependam dele
    protected override bool CheckShouldGlowByDefault()
    {
        if (TaskManager.Instance == null) return false;
        return TaskManager.Instance.HasActiveTask("Escrever documento") ||
               TaskManager.Instance.HasActiveTask("Imprimir documento");
    }
}
