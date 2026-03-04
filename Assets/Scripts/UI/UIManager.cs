using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour {
    public GameObject tooltipObject;
    public GameObject openLockView;
    public TextMeshProUGUI lockDisplayText;
    public Image redLed;
    public Image greenLed;
    public Button[] lockButtons;
    private LockScript currentLock;


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