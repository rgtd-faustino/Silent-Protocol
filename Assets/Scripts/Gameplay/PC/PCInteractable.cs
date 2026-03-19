using UnityEngine;

public class PCInteractable : InteractableObject
{
    public GameObject pcCanvas;

    private bool isOpen = false;

    public override void Interact()
    {
        if (!isOpen)
            OpenPC();
        else
            ClosePC();
    }

    void OpenPC()
    {
        pcCanvas.SetActive(true);

        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);

        isOpen = true;
    }
    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePC();
        }
    }

    void ClosePC()
    {
        pcCanvas.SetActive(false);

        PlayerController.Instance.canMoveRotate = true;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);

        isOpen = false;
    }

}
