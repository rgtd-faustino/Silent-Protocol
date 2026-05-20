using UnityEngine;

public class LockScript : InteractableObject
{

    // estado de visibilidade da view do keypad
    // se o jogador fechar a view pelo boto de sair, o UIManager chama SyncViewClosed() para repor esta flag a false sem chamar Interact()
    private bool isViewOpen = false;

    // definido no Inspector para cada fechadura
    public int[] code = new int[5];

    private Rigidbody rb;

    // estado da fechadura  lido pelo DoorScript para saber se pode abrir
    public bool isLocked;


    protected override void Awake()
    {
        base.Awake();
        objectName = "Fechadura de Código";
        tooltipMessage = "E para introduzir código";
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // se a fechadura comea destrancada, retira-a da layer Interactable e desativa o script 
        // caso contrrio o raycast do jogador deteta a fechadura antes da porta, forando sempre dois cliques
        if (!isLocked)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            enabled = false;
        }
    }


    // a view e os bloqueios de input so geridos no UIManager
    public override void Interact()
    {
        isViewOpen = !isViewOpen;

        if (isViewOpen)
        {
            UIManager.Instance.OpenLockView(this); // passa-se a si prprio para o UIManager poder chamar TryCode()
            UIManager.Instance.ChangeCursorState(CursorLockMode.Confined); // cursor visvel mas preso ao ecr
            PlayerController.Instance.canMoveRotate = false;

        }
        else
        {
            UIManager.Instance.CloseLockView();
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
        }
    }

    // chamado pelo UIManager quando o jogador carrega no boto de sair da view em vez de adivinhar o cdigo
    public void SyncViewClosed()
    {
        isViewOpen = false;
    }


    // chamado pelo UIManager com o array de 5 dgitos introduzidos
    // quando correto: destrancar, remover da layer Interactable e desativar o script (a fechadura j no  interatvel)
    public bool TryCode(int[] attempt)
    {
        for (int i = 0; i < code.Length; i++)
        {
            if (attempt[i] != code[i])
            {
                Debug.Log("Cdigo errado!");
                return false;
            }
        }

        isLocked = false;
        isViewOpen = false;

        // remover da layer "Interactable" para que o raycast do jogador deixe de detetar esta fechadura como interactable
        gameObject.layer = LayerMask.NameToLayer("Default");

        // desativar o script porque j no h nada para gerir
        enabled = false;

        Debug.Log("Porta destrancada!");
        return true;
    }

    // ativa a gravidade aps o delay visual do LED verde (gerido no UIManager), a fechadura cai fisicamente do stio como feedback ao jogador.
    public void DropLock()
    {
        rb.useGravity = true;
    }
}