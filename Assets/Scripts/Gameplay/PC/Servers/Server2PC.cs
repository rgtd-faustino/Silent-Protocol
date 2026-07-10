using UnityEngine;

public class Server2PC : InteractableObject
{
    public GameObject pcCanvas;
    [SerializeField] private DialogueCutscene srv2Cutscene;
    
    // Garante que a cutscene de introdução ao servidor toca apenas na primeira interação
    private bool cutsceneTriggered = false;
    
    // Impede problemas de concorrência na entrada de input e duplicação da interface
    private bool isOpen = false;

    protected override void Awake()
    {
        base.Awake();
        objectName = "Terminal do Servidor 2";
        tooltipMessage = "E para aceder ao Servidor 2";
    }

    // Interceta a interação inicial para arrancar o diálogo com a CutsceneDialogueUI e depois delega para a abertura do painel
    public override void Interact()
    {
        if (!cutsceneTriggered)
        {
            cutsceneTriggered = true;
            CutsceneDialogueUI.Instance.Play(srv2Cutscene, CutsceneDialogueUI.PanelTarget.Srv2, () =>
            {
                OpenPC();
            }, false);
        }
        else
        {
            if (!isOpen)
                OpenPC();
            else
                ClosePC();
        }
    }

    // Trava o PlayerController e liberta o rato para o jogador conseguir clicar na interface do servidor
    private void OpenPC()
    {
        pcCanvas.SetActive(true);
        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        isOpen = true;

        Server2UI srv2 = pcCanvas.GetComponentInChildren<Server2UI>();
        if (srv2 != null) srv2.OnOpen();
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.P))
            ClosePC();
    }

    // Remove o painel do servidor e restaura os controlos e a câmara normais
    private void ClosePC()
    {
        pcCanvas.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
        isOpen = false;
    }
}