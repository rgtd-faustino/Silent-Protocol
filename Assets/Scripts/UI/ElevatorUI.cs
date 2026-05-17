using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElevatorUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform elevatorCar;
    public TextMeshProUGUI txtSysStatus;
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtStatusPos;
    public TextMeshProUGUI txtStatusDest;
    public Button confirmButton;
    public TextMeshProUGUI txtConfirm;

    [Header("Detail Panel")]
    public TextMeshProUGUI detTitle;
    public TextMeshProUGUI detSubtitle;
    public TextMeshProUGUI detDesc;
    public TextMeshProUGUI detFeatLabel;
    public TextMeshProUGUI detFeat1;
    public TextMeshProUGUI detFeat2;
    public TextMeshProUGUI detFeat3;
    public GameObject detLocked;

    [Header("Floor Buttons")]
    public Button[] floorButtons; // index 0 = F5, 1 = F4, ..., 4 = F1

    [Header("Floor Spawns")]
    public Transform spawnFloor1;
    public Transform spawnFloor2;
    public Transform spawnFloor3;
    public Transform spawnFloor4;
    public Transform spawnFloor5;

    [Header("Settings")]
    public float carMoveSpeed = 1.3f;
    public float floorHeight = 64f;

    // Estado
    private int currentFloor = 1;
    private int selectedFloor = -1;
    private bool isMoving = false;
    private const string TitleFullText = "// SELEÇÃO DE DESTINO_";
    private const float CharDelay = 0.045f;

    // Dados dos pisos
    private FloorData[] floors;

    public static class ElevatorColors
    {
        public static readonly Color32 Green = new Color32(0, 200, 100, 255);
        public static readonly Color32 Amber = new Color32(255, 176, 0, 255);
        public static readonly Color32 Red = new Color32(255, 51, 51, 255);
        public static readonly Color32 DarkBg = new Color32(10, 13, 20, 255);
        public static readonly Color32 PanelBg = new Color32(12, 15, 22, 255);
        public static readonly Color32 Border = new Color32(24, 48, 32, 255);
        public static readonly Color32 Muted = new Color32(46, 84, 56, 255);
        public static readonly Color32 Inactive = new Color32(30, 48, 32, 255);
    }

    [System.Serializable]
    public class FloorData
    {
        public int floorNumber;
        public string label;
        public string fullName;
        public string description;
        public string access;
        public string clearance;
        public string[] features;
        public bool locked;

        public FloorData(int num, string lbl, string full, string desc, string acc, string clr, string[] feats, bool locked)
        {
            floorNumber = num;
            label = lbl;
            fullName = full;
            description = desc;
            access = acc;
            clearance = clr;
            features = feats;
            this.locked = locked;
        }
    }

    void Awake()
    {
        floors = new FloorData[]
        {
            new FloorData(5, "CEO", "PISO DO CEO", "Acesso reservado ao CEO. Credencial biométrica obrigatória.",
                "BLOQUEADO", "NÍVEL-5 MÁXIMO", new[]{"SALA DE REUNIÕES EXECUTIVA","COFRE DE DADOS","TERMINAL SEGURO"},
                locked: true),

            new FloorData(4, "SUÍTES", "ANDAR DAS SUÍTES", "Dormitórios. Infiltração noturna. Risco elevado — câmaras e guardas.",
                "RESTRITO",  "NÍVEL-3 REQUERIDO", new[]{"DORMITÓRIOS PRIVADOS","ARMAZENAMENTO PESSOAL","CREDENCIAIS"},
                locked: false),

            new FloorData(3, "SERVIDORES", "ANDAR DOS SERVIDORES", "Centro técnico. Alto risco. Patrulhas frequentes. Câmaras 24h.",
                "BLOQUEADO", "NÍVEL-4 REQUERIDO", new[]{"RACKS DE SERVIDORES","TERMINAIS TÉCNICOS","PUZZLES DE REDE"},
                locked: true),

            new FloorData(2, "EXECUTIVO",  "ANDAR EXECUTIVO", "Área principal de trabalho. Manter aparência produtiva.",
                "LIVRE",     "NÍVEL-1 PADRÃO", new[]{"POSTO DE TRABALHO","SALA DE REUNIÕES","IMPRESSORAS / FAX"},
                locked: false),

            new FloorData(1, "RECEÇÃO", "RECEÇÃO", "Entrada do edifício. Controlo de visitas. Baixo risco.",
                "LIVRE", "NÍVEL-1 PADRÃO", new[]{"CONTROLO DE VISITAS","AGENDAS PÚBLICAS","INFORMAÇÃO SOCIAL"},
                locked: false)
        };

        SetCarPosition(currentFloor, true);
        ClearDetails();
        SyncLockedStates();
    }

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        StartCoroutine(TypewriterTitle());
        PlayerController.Instance.canMoveRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        selectedFloor = -1;
        ClearDetails();
        confirmButton.interactable = false;
        PlayerController.Instance.canMoveRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnFloorClicked(int index)
    {
        if (isMoving)
            return;

        FloorData data = floors[index];

        if (data.floorNumber == currentFloor)
            return;

        selectedFloor = index;

        // Move o carro visualmente para o piso selecionado
        StopAllCoroutines();
        StartCoroutine(MoveCarTo(data.floorNumber, false));

        // Atualiza status
        txtStatusDest.text = $"DESTINO: F{data.floorNumber} — {data.label}";
        txtStatusDest.color = data.locked ? ElevatorColors.Red : ElevatorColors.Green;

        // Atualiza painel de detalhes
        ShowDetails(data);

        // Botão confirmar
        confirmButton.interactable = !data.locked;
    }

    public void OnConfirm()
    {
        if (selectedFloor < 0 || floors[selectedFloor].locked || isMoving)
            return;

        StartCoroutine(DoTravel());
    }

    IEnumerator DoTravel()
    {
        isMoving = true;
        FloorData dest = floors[selectedFloor];

        txtConfirm.text = "> A MOVER...";
        confirmButton.interactable = false;
        txtSysStatus.text = "SHAFT-A // EM MOVIMENTO";

        ShowTransit(currentFloor, dest.floorNumber);

        yield return new WaitForSecondsRealtime(carMoveSpeed + 0.3f);

        currentFloor = dest.floorNumber;
        selectedFloor = -1;
        isMoving = false;

        txtConfirm.text = "> CONFIRMAR";
        txtSysStatus.text = "SHAFT-A // ATIVO";
        txtStatusPos.text = $"POSIÇÃO ATUAL: F{currentFloor} — {dest.label}";
        txtStatusDest.text = "DESTINO: --";
        txtStatusDest.color = ElevatorColors.Muted;

        ShowArrived(dest);

        // Aqui chamas o teu GameManager para mudar de piso
        GameManager.Instance.SetCurrentFloor(currentFloor - 1);
        // Podes fechar o UI depois de X segundos ou esperar input
        yield return new WaitForSecondsRealtime(0.8f);
        TeleportToFloor(currentFloor);
        Close();
    }

    IEnumerator MoveCarTo(int targetFloor, bool instant)
    {
        float startY = elevatorCar.anchoredPosition.y;
        float targetY = FloorToY(targetFloor);
        float elapsed = 0f;
        float duration = instant ? 0f : carMoveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            Vector2 pos = elevatorCar.anchoredPosition;
            pos.y = Mathf.Lerp(startY, targetY, t);
            elevatorCar.anchoredPosition = pos;
            yield return null;
        }

        Vector2 final = elevatorCar.anchoredPosition;
        final.y = targetY;
        elevatorCar.anchoredPosition = final;
    }

    float FloorToY(int floorNumber)
    {
        switch (floorNumber)
        {
            case 5:
                return -33.3f;

            case 4:
                return -98.3f;

            case 3:
                return -162.3f;

            case 2:
                return -226.3f;

            case 1:
                return -290.3f;

            default:
                return -226.3f;
        }
    }

    void SetCarPosition(int floorNumber, bool instant)
    {
        if (instant)
        {
            Vector2 pos = elevatorCar.anchoredPosition;
            pos.y = FloorToY(floorNumber);
            elevatorCar.anchoredPosition = pos;

        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(MoveCarTo(floorNumber, instant: false));
        }
    }

    void ShowDetails(FloorData d)
    {
        Color color = d.locked ? ElevatorColors.Red : d.access == "RESTRITO" ? ElevatorColors.Amber : ElevatorColors.Green;

        detTitle.text = $"F{d.floorNumber} — {d.fullName}";
        detTitle.color = color;
        detSubtitle.text = $"{d.access}  //  {d.clearance}";
        detSubtitle.color = color;
        detDesc.text = d.description;
        detFeatLabel.text = "INFRAESTRUTURA DISPONÍVEL:";
        detFeat1.text = $"— {d.features[0]}";
        detFeat2.text = $"— {d.features[1]}";
        detFeat3.text = $"— {d.features[2]}";
        detLocked.SetActive(d.locked);
    }

    void ShowTransit(int from, int to)
    {
        detTitle.text = "EM TRÂNSITO";
        detTitle.color = ElevatorColors.Green;
        detSubtitle.text = $"F{from} → F{to}";
        detSubtitle.color = ElevatorColors.Muted;
        detDesc.text = "";
        detFeatLabel.text = "";
        detFeat1.text = detFeat2.text = detFeat3.text = "";
        detLocked.SetActive(false);
    }

    void ShowArrived(FloorData d)
    {
        detTitle.text = $"CHEGASTE AO F{d.floorNumber}";
        detTitle.color = ElevatorColors.Green;
        detSubtitle.text = d.label;
        detSubtitle.color = ElevatorColors.Muted;
        detDesc.text = detFeatLabel.text = "";
        detFeat1.text = detFeat2.text = detFeat3.text = "";
        detLocked.SetActive(false);
    }

    void TeleportToFloor(int floorNumber)
    {
        Transform target;

        switch (floorNumber)
        {
            case 1:
                target = spawnFloor1;
                break;

            case 2:
                target = spawnFloor2;
                break;

            case 3:
                target = spawnFloor3;
                break;

            case 4:
                target = spawnFloor4;
                break;

            case 5:
                target = spawnFloor5;
                break;

            default:
                target = spawnFloor1;
                break;
        }


        CharacterController cc = PlayerController.Instance.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerController.Instance.transform.position = target.position;
        PlayerController.Instance.transform.rotation = target.rotation;
        cc.enabled = true;
    }

    IEnumerator TypewriterTitle()
    {
        string textWithoutCursor = "// SELEÇÃO DE DESTINO";
        txtTitle.text = "";
        foreach (char c in textWithoutCursor)
        {
            txtTitle.text += c;
            yield return new WaitForSecondsRealtime(CharDelay);
        }
        StartCoroutine(BlinkCursor());
    }

    IEnumerator BlinkCursor()
    {
        string textWithoutCursor = "// SELEÇÃO DE DESTINO";
        bool cursorVisible = true;
        while (gameObject.activeSelf)
        {
            txtTitle.text = cursorVisible ? textWithoutCursor + "_" : textWithoutCursor;
            cursorVisible = !cursorVisible;
            yield return new WaitForSecondsRealtime(0.52f);
        }
    }

    void ClearDetails()
    {
        detTitle.text = detSubtitle.text = detDesc.text = "";
        detFeatLabel.text = "";
        detFeat1.text = detFeat2.text = detFeat3.text = "";
        detLocked.SetActive(false);
    }

    // Método público para o GameManager desbloquear pisos
    public void UnlockFloor(int floorNumber)
    {
        int index = 5 - floorNumber;
        if (index >= 0 && index < floors.Length)
            floors[index].locked = false;
    }

    public void SyncLockedStates()
    {
        foreach (var floor in floors)
        {
            int gmIndex = floor.floorNumber - 1;
            floor.locked = !GameManager.Instance.IsFloorUnlocked(gmIndex);
        }
    }
}