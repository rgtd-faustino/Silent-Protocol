using UnityEngine;

public class PCInteractable : InteractableObject
{
    public GameObject pcCanvas;
    private bool isOpen = false;

    [Header("Segurança")]
    public bool precisaDeCartao = false;
    public string cardID;
    public string cardName;

    public bool precisaDePin = false;
    public string pinCorreto = "1234";

    // fica true assim que os requisitos forem cumpridos uma vez — não volta a pedir depois disso
    // (mesma filosofia do CardReader: uma vez desbloqueado, fica desbloqueado)
    private bool isUnlocked = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Computador";
        tooltipMessage = "E para usar computador";
    }

    public override void Interact()
    {
        if (isOpen)
        {
            ClosePC();
            return;
        }

        // se já desbloqueou antes, ou se nenhum boolean está ativo, entra logo
        if (isUnlocked || (!precisaDeCartao && !precisaDePin))
        {
            OpenPC();
            return;
        }

        TentarDesbloquear();
    }

    private void TentarDesbloquear()
    {
        // 1) cartão primeiro, porque é uma verificação instantânea (sem UI)
        if (precisaDeCartao && !PlayerController.Instance.HasCardCredential(cardID))
        {
            StartCoroutine(AvisoCartaoEmFalta());
            return;
        }

        // 2) se também precisar de pin, só agora abre o teclado
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

        // só precisava de cartão, e o cartão está ok
        isUnlocked = true;
        OpenPC();
    }

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

        // reset ao estado do servidor se existir
        Server1UI srv1 = pcCanvas.GetComponentInChildren<Server1UI>();
        if (srv1 != null) srv1.OnOpen();
        Server2UI srv2 = pcCanvas.GetComponentInChildren<Server2UI>();
        if (srv2 != null) srv2.OnOpen();
    }

    private void Update()
    {
        // s tenta apanahr o Escape se o PC estiver aberto
        if (isOpen && Input.GetKeyDown(KeyCode.P))
            ClosePC();
    }

    private void ClosePC()
    {
        pcCanvas.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isOpen = false;
    }

    protected override bool CheckShouldGlowByDefault()
    {
        if (TaskManager.Instance == null) return false;
        return TaskManager.Instance.HasActiveTask("Escrever documento") ||
               TaskManager.Instance.HasActiveTask("Imprimir documento");
    }
}
