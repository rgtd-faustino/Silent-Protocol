using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Server1UI : MonoBehaviour
{
    public static Server1UI Instance;

    [Header("Paineis")]
    [SerializeField] private GameObject serverPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel;

    [Header("Login")]
    [SerializeField] private TMP_InputField userInput;
    [SerializeField] private TMP_InputField passInput;
    [SerializeField] private GameObject errorText;

    [Header("Tabs")]
    [SerializeField] private Button tabAccessLogs;
    [SerializeField] private Button tabBackups;
    [SerializeField] private Button tabSafeIn;
    [SerializeField] private GameObject panelAccessLogs;
    [SerializeField] private GameObject panelBackups;
    [SerializeField] private GameObject panelSafeIn;

    [Header("Cutscene Access Logs")]
    [SerializeField] private DialogueCutscene accessLogsCutscene;
    private bool accessLogsCutsceneTriggered = false;

    private const string SRV1_USER = "srv_admin_b";
    private const string SRV1_PASS = "C0rp#2047!Srv";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        tabAccessLogs.onClick.AddListener(() => SwitchTab(0));
        tabBackups.onClick.AddListener(() => SwitchTab(1));
        tabSafeIn.onClick.AddListener(() => SwitchTab(2));
    }

    public void OnOpen()
    {
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
        errorText.SetActive(false);
        userInput.text = "";
        passInput.text = "";
    }

    public void TryLogin()
    {
        if (userInput.text.Trim() == SRV1_USER && passInput.text.Trim() == SRV1_PASS)
        {
            // login correto
            loginPanel.SetActive(false);
            mainPanel.SetActive(true);
            SwitchTab(0);
        }
        else
        {
            // login errado  mostra erro e esconde aps 2 segundos
            StartCoroutine(ShowError());
        }
    }

    private IEnumerator ShowError()
    {
        errorText.SetActive(true);
        yield return new WaitForSeconds(2f);
        errorText.SetActive(false);
    }

    public void SwitchTab(int index)
    {
        panelAccessLogs.SetActive(index == 0);
        panelBackups.SetActive(index == 1);
        panelSafeIn.SetActive(index == 2);

        if (index == 0 && !accessLogsCutsceneTriggered && accessLogsCutscene != null)
        {
            accessLogsCutsceneTriggered = true;
            CutsceneDialogueUI.Instance.Play(accessLogsCutscene, CutsceneDialogueUI.PanelTarget.Srv1, null, false);
        }
    }
}