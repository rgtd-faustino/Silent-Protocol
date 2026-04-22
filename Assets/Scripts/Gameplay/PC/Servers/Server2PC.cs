using UnityEngine;

public class Server2PC : InteractableObject
{
    public GameObject pcCanvas;
    [SerializeField] private DialogueCutscene srv2Cutscene;
    private bool cutsceneTriggered = false;
    private bool isOpen = false;

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

    private void ClosePC()
    {
        pcCanvas.SetActive(false);
        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
        isOpen = false;
    }
}