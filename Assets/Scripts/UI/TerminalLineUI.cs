using TMPro;
using UnityEngine;

public class TerminalLineUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI lineText;

    public void SetLine(string text, Color color)
    {
        lineText.text = text;
        lineText.color = color;
    }
}