using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [Header("Tooltip")]
    public GameObject tooltipObject;

    [Header("Lock")]
    public GameObject openLockView;
    public TextMeshProUGUI lockDisplayText;
    public Image redLed;
    public Image greenLed;
    public Button[] lockButtons;
    private LockScript currentLock;

    [Header("Sleep")]
    public GameObject sleepUI;
    public GameObject sleepInputPanel;       // painel com input field e botões confirmar/cancelar
    public GameObject sleepAnimPanel;        // painel do relógio radial — só aparece na animação
    public TMP_InputField sleepHoursInput;
    public TextMeshProUGUI sleepHoursTextError;
    public Image sleepRadialClock;           // Image com Fill Type=Filled, Radial 360, anti-clockwise
    public TextMeshProUGUI sleepCountdownText;
    private BedScript currentBed;

    [Header("PC")]
    // botão de imprimir no ecrã do computador — só deve estar ativo quando a task de impressão existir
    [SerializeField] private Button printButton;

    private int[] currentCodeTry = new int[5];
    private int currentIndexDigit = 0;

    public static UIManager Instance;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ChangeCursorState(CursorLockMode mode) {
        Cursor.lockState = mode;
        Input.ResetInputAxes(); // previne spike de rato no frame da transição
    }

    // atualiza os controlos do PC para refletir o estado atual do jogo
    // chamado pelo PCInteractable quando o PC é aberto
    public void RefreshPCInterface() {
        if (printButton != null)
            printButton.interactable = TaskManager.Instance.HasActiveMorningTask("Imprimir documento");
    }

    // ---- Sleep ----

    public void ShowSleepUI() => sleepUI.SetActive(true);
    public void HideSleepUI() => sleepUI.SetActive(false);

    public void OpenSleepView(BedScript bed) {
        currentBed = bed;

        sleepHoursTextError.gameObject.SetActive(false);
        sleepHoursTextError.text = "";
        sleepHoursInput.text = "";
        sleepRadialClock.fillAmount = 1f;

        sleepInputPanel.SetActive(true);
        sleepAnimPanel.SetActive(false);
        sleepUI.SetActive(true);

        ChangeCursorState(CursorLockMode.None);
        PlayerController.Instance.canMoveRotate = false;
    }

    public void ConfirmSleep() {
        string raw = sleepHoursInput.text.Trim();
        string[] parts = raw.Split(':'); // o jogador introduz sempre no formato HH:MM

        if (parts.Length != 2 || !int.TryParse(parts[0], out int h) || !int.TryParse(parts[1], out int m)) {
            ShowSleepError("Formato inválido. Usa HH:MM (ex: 07:30)");
            return;
        }

        if (h < 0 || h > 23 || m < 0 || m >= 60) {
            ShowSleepError("Hora inválida");
            return;
        }

        float currentTimeInHours = TimeManager.Instance.GetCurrentTimeInHours();
        float wakeUpTimeInHours = h + m / 60f;

        // se a hora de acordar já passou hoje, assume-se que é amanhã
        if (wakeUpTimeInHours <= currentTimeInHours)
            wakeUpTimeInHours += 24f;

        float hours = wakeUpTimeInHours - currentTimeInHours;
        float maxHours = TimeManager.Instance.GetMaxSleepHours();

        if (hours > maxHours) {
            int maxH = Mathf.FloorToInt(maxHours);
            int maxM = Mathf.FloorToInt((maxHours - maxH) * 60f);
            ShowSleepError($"Não podes acordar após as 08:00. Máximo {maxH}h {maxM:00}m de sono");
            return;
        }

        sleepHoursTextError.gameObject.SetActive(false);
        sleepHoursTextError.text = "";
        StartCoroutine(SleepSequence(hours, wakeUpTimeInHours));
    }

    private void ShowSleepError(string message) {
        sleepHoursTextError.gameObject.SetActive(true);
        sleepHoursTextError.text = message;
        sleepHoursInput.text = "";
    }

    public void CancelSleep() {
        currentBed = null;
        sleepUI.SetActive(false);
        ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;
    }

    private IEnumerator SleepSequence(float hours, float wakeUpTimeInHours) {
        sleepInputPanel.SetActive(false);
        sleepAnimPanel.SetActive(true);

        float duration = 3f; // duração real da animação em segundos
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // relógio esvazia anti-horário: fillAmount de 1 → 0
            sleepRadialClock.fillAmount = 1f - t;

            if (sleepCountdownText != null) {
                float hoursLeft = hours * (1f - t);
                int hLeft = Mathf.FloorToInt(hoursLeft);
                int mLeft = Mathf.FloorToInt((hoursLeft - hLeft) * 60f);
                sleepCountdownText.text = $"{hLeft}h {mLeft:00}m";
            }

            yield return null;
        }

        sleepRadialClock.fillAmount = 0f;
        if (sleepCountdownText != null)
            sleepCountdownText.text = "0h 00m";

        yield return new WaitForSeconds(0.5f);

        // notifica a cama (hook para sons/animações futuras) e aplica o sono no TimeManager
        currentBed?.OnSleepConfirmed(hours);
        TimeManager.Instance.Sleep(wakeUpTimeInHours);

        sleepUI.SetActive(false);
        ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;
        currentBed = null;
    }

    // ---- Tooltip ----

    public void ShowTooltip() => tooltipObject.SetActive(true);
    public void HideTooltip() => tooltipObject.SetActive(false);

    // ---- Lock ----

    public void OpenLockView(LockScript lockScript) {
        currentLock = lockScript;
        currentIndexDigit = 0;
        currentCodeTry = new int[5];
        openLockView.SetActive(true);
        UpdateLockDisplay();
    }

    public void CloseLockView() {
        currentLock = null;
        openLockView.SetActive(false);
    }

    public bool IsLockViewOpen() => openLockView.activeSelf;

    public void OnDigitPressed(int digit) {
        if (currentLock == null) return;

        if (digit == -1) {
            // botão de sair — fecha a view sem tentar o código
            currentLock.SyncViewClosed();
            CloseLockView();
            ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
            return;
        }

        if (digit == -2) {
            // botão de apagar — remove o último dígito introduzido
            if (currentIndexDigit > 0) {
                currentIndexDigit--;
                UpdateLockDisplay();
            }
            return;
        }

        if (currentIndexDigit >= 5) return;

        currentCodeTry[currentIndexDigit++] = digit;
        UpdateLockDisplay();

        if (currentIndexDigit == 5) {
            bool correct = currentLock.TryCode(currentCodeTry);
            SetLed(correct ? greenLed : redLed, 1f);
            SetButtonsInteractable(false);
            StartCoroutine(correct ? CorrectCodeDelay() : WrongCodeDelay());
        }
    }

    private void UpdateLockDisplay() {
        string display = "";
        for (int i = 0; i < 5; i++)
            display += i < currentIndexDigit ? currentCodeTry[i].ToString() : "*";
        lockDisplayText.text = display;
    }

    private void SetLed(Image led, float alpha) {
        Color c = led.color;
        c.a = alpha;
        led.color = c;
    }

    private void SetButtonsInteractable(bool interactable) {
        foreach (Button b in lockButtons)
            b.interactable = interactable;
    }

    private IEnumerator CorrectCodeDelay() {
        yield return new WaitForSeconds(1f);

        currentLock.DropLock();
        CloseLockView();
        ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;

        SetLed(redLed, 0.5f);
        SetLed(greenLed, 0.5f);
        ResetInput();
    }

    private IEnumerator WrongCodeDelay() {
        yield return new WaitForSeconds(1f);

        SetLed(redLed, 0.5f);
        SetLed(greenLed, 0.5f);
        ResetInput();
        UpdateLockDisplay();
        SetButtonsInteractable(true);
    }

    private void ResetInput() {
        currentIndexDigit = 0;
        currentCodeTry = new int[5];
    }


    // o botão de imprimir no computador só deve funcionar quando a task está ativa
    public void OnPrinterPrintButton() {
        if (!TaskManager.Instance.HasActiveMorningTask("Imprimir documento")) 
            return;

        TaskManager.Instance.ActivatePrinterTask();
        printButton.interactable = false;
    }
}