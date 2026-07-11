using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneDialogueUI : MonoBehaviour
{
    public static CutsceneDialogueUI Instance;

    [Header("Painel Principal (entrada, NPCs)")]
    public GameObject mainPanel;
    public TextMeshProUGUI mainSpeakerText;
    public TextMeshProUGUI mainDialogueText;

    [Header("Painel Servidor 1")]
    public GameObject srv1Panel;
    public TextMeshProUGUI srv1SpeakerText;
    public TextMeshProUGUI srv1DialogueText;

    [Header("Painel Servidor 2")]
    public GameObject srv2Panel;
    public TextMeshProUGUI srv2SpeakerText;
    public TextMeshProUGUI srv2DialogueText;

    private DialogueLine[] lines;
    private int currentLine = 0;
    private System.Action onComplete;

    private GameObject activePanel;
    private TextMeshProUGUI activeSpeaker;
    private bool lockCursorOnEnd = true;
    private TextMeshProUGUI activeDialogue;

    public enum PanelTarget { Main, Srv1, Srv2 }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainPanel) mainPanel.SetActive(false);
        if (srv1Panel) srv1Panel.SetActive(false);
        if (srv2Panel) srv2Panel.SetActive(false);
    }

    public void Play(DialogueCutscene cutscene, System.Action onDone = null)
    {
        Play(cutscene, PanelTarget.Main, onDone, true);
    }

    public void Play(DialogueCutscene cutscene, PanelTarget target, System.Action onDone, bool lockOnEnd)
    {
        lockCursorOnEnd = lockOnEnd;
        lines = cutscene.lines;
        currentLine = 0;
        onComplete = onDone;

        switch (target)
        {
            case PanelTarget.Srv1:
                activePanel = srv1Panel;
                activeSpeaker = srv1SpeakerText;
                activeDialogue = srv1DialogueText;
                break;

            case PanelTarget.Srv2:
                activePanel = srv2Panel;
                activeSpeaker = srv2SpeakerText;
                activeDialogue = srv2DialogueText;
                break;

            default:
                activePanel = mainPanel;
                activeSpeaker = mainSpeakerText;
                activeDialogue = mainDialogueText;
                break;
        }

        PlayerController.Instance.canMoveRotate = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.None);
        activePanel.SetActive(true);
        ShowLine(currentLine);
    }

    public void Next()
    {
        currentLine++;
        if (currentLine >= lines.Length)
        {
            End();
            return;
        }
        ShowLine(currentLine);
    }

    private void ShowLine(int index)
    {
        activeSpeaker.text = lines[index].speakerName;
        activeDialogue.text = lines[index].text;
    }

    private void End()
    {
        activePanel.SetActive(false);

        if (lockCursorOnEnd)
        {
            PlayerController.Instance.canMoveRotate = true;
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
        }

        onComplete?.Invoke();
    }
}