using System.Collections;
using System.Text.RegularExpressions;
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
    public GameObject sleepInputPanel; // painel com o input field e botões confirmar/cancelar
    public GameObject sleepAnimPanel; // painel com o relógio radial (aparece só na animação)
    public TMP_InputField sleepHoursInput;
    public TextMeshProUGUI sleepHoursTextError;
    public Image sleepRadialClock; // Image com Fill Type=Filled, Radial 360, anti-clockwise
    public TextMeshProUGUI sleepCountdownText; // opcional: mostra "7h 30m" a diminuir
    private BedScript currentBed;

    private int[] currentCodeTry = new int[5];
    private int currentIndexDigit = 0;


    public static UIManager Instance;


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Cursor.lockState = CursorLockMode.Locked; // esconde o cursor no início
    }



    public void ChangeCursorState(CursorLockMode mode) {
        Cursor.lockState = mode;
        Input.ResetInputAxes(); // às vezes temos um spike de rato no frame da transição isto arranja
    }



    // Update is called once per frame
    void Update() {

    }

    public void ShowSleepUI() {
        sleepUI.SetActive(true);
    }

    public void HideSleepUI() {
        sleepUI.SetActive(false);
    }


    public void OpenSleepView(BedScript bed) {
        currentBed = bed;

        // dar reset a tudo e só depois mostramos as coisas
        sleepHoursTextError.gameObject.SetActive(false);
        sleepHoursTextError.text = "";
        sleepHoursInput.text = "";
        sleepRadialClock.fillAmount = 1f;

        sleepInputPanel.SetActive(true);
        sleepAnimPanel.SetActive(false);
        sleepUI.SetActive(true);

        ChangeCursorState(CursorLockMode.None); // deixar o jogador mexer o quadro para poder escrever no campo de input
        PlayerController.Instance.canMoveRotate = false;
    }

    public void ConfirmSleep() {
        string raw = sleepHoursInput.text.Trim();
        string[] parts = raw.Split(':'); // o jogador mete sempre em formato HH:MM então dividimos aí

        if (parts.Length != 2 || !int.TryParse(parts[0], out int h) || !int.TryParse(parts[1], out int m)) {
            ShowSleepError("Formato inválido. Usa HH:MM (ex: 08:30)");
            return;
        }

        if (h < 0 || m < 0 || m >= 60) {
            ShowSleepError("Horas entre 0-23, minutos entre 00-59");
            return;
        }

        float hours = h + m / 60f;
        float maxHours = TimeManager.Instance.GetMaxSleepHours();

        if (hours <= 0f) {
            ShowSleepError("Tens de dormir pelo menos alguns minutos");
            return;
        }

        if (hours > maxHours) {
            int maxH = Mathf.FloorToInt(maxHours);
            int maxM = Mathf.FloorToInt((maxHours - maxH) * 60f);
            ShowSleepError($"Não podes dormir mais de {maxH}h {maxM:00}m, acordarias depois das 08:00");
            return;
        }

        sleepHoursTextError.gameObject.SetActive(false);
        sleepHoursTextError.text = "";
        StartCoroutine(SleepSequence(hours));
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

    private IEnumerator SleepSequence(float hours) {
        // troca para o painel da animação
        sleepInputPanel.SetActive(false);
        sleepAnimPanel.SetActive(true);

        float duration = 3f;   // duração real da animação em segundos (ajustável)
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // relógio esvazia no sentido anti-horário: fillAmount de 1 → 0
            sleepRadialClock.fillAmount = 1f - t;

            // texto opcional com as horas restantes
            if (sleepCountdownText != null) {
                float hoursLeft = hours * (1f - t);
                int h = Mathf.FloorToInt(hoursLeft);
                int m = Mathf.FloorToInt((hoursLeft - h) * 60f);
                sleepCountdownText.text = $"{h}h {m:00}m";
            }

            yield return null;
        }

        sleepRadialClock.fillAmount = 0f;

        if (sleepCountdownText != null)
            sleepCountdownText.text = "0h 00m";

        yield return new WaitForSeconds(0.5f);

        // aplica o sono
        if (currentBed != null)
            currentBed.OnSleepConfirmed(hours);

        // fecha tudo
        sleepUI.SetActive(false);
        ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;
        currentBed = null;
    }

    public void ShowTooltip() {
        tooltipObject.SetActive(true);
    }

    public void HideTooltip() {
        tooltipObject.SetActive(false);
    }

    public void OpenLockView(LockScript lockScript) {
        currentLock = lockScript;
        currentIndexDigit = 0; // reset da tentativa anterior
        currentCodeTry = new int[5];
        openLockView.SetActive(true);
        UpdateLockDisplay();
    }

    private void UpdateLockDisplay() {
        string display = "";

        for (int i = 0; i < 5; i++)
            display += i < currentIndexDigit ? currentCodeTry[i].ToString() : "*";

        lockDisplayText.text = display;
    }


    public void CloseLockView() {
        currentLock = null;
        openLockView.SetActive(false);
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

    public bool IsLockViewOpen() {
        return openLockView.activeSelf;
    }

    public void OnDigitPressed(int digit) {
        if (currentLock == null)
            return;

        // se clicou no botão de sair fecha a view e volta a jogar
        if (digit == -1) {
            currentLock.SyncViewClosed();
            CloseLockView();
            ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
            return;
        }

        // se clicou no botão de apagar remove o último dígito introduzido
        if (digit == -2) {
            if (currentIndexDigit > 0) {
                currentIndexDigit--;
                UpdateLockDisplay();
            }
            return;
        }

        // se já tem 5 dígitos introduzidos
        if (currentIndexDigit >= 5)
            return;

        currentCodeTry[currentIndexDigit++] = digit;
        UpdateLockDisplay();

        if (currentIndexDigit == 5) {
            bool correct = currentLock.TryCode(currentCodeTry);
            SetLed(correct ? greenLed : redLed, 1f);
            SetButtonsInteractable(false); // desativa botões durante o delay
            StartCoroutine(correct ? CorrectCodeDelay() : WrongCodeDelay());
        }
    }

    private IEnumerator CorrectCodeDelay() {
        yield return new WaitForSeconds(1f);

        currentLock.DropLock(); // cai após o delay
        CloseLockView();
        ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;

        // reset leds e display para quando a view abrir noutra porta
        SetLed(redLed, 0.5f);
        SetLed(greenLed, 0.5f);
        ResetInput();
    }

    private IEnumerator WrongCodeDelay() {
        yield return new WaitForSeconds(1f);

        SetLed(redLed, 0.5f);
        SetLed(greenLed, 0.5f);
        ResetInput();
        UpdateLockDisplay(); // reset do texto após o delay
        SetButtonsInteractable(true); // volta a ativar os botões
    }


    private void ResetInput() {
        currentIndexDigit = 0;
        currentCodeTry = new int[5];
    }




}