using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryRowUI : MonoBehaviour
{
    public TextMeshProUGUI idText, uidText, encText, riskText;
    public Image riskBg;
    public Button ackButton;

    private static readonly Color ColLow = new Color(0.27f, 0.6f, 0.27f);
    private static readonly Color ColMed = new Color(0.8f, 0.53f, 0.1f);
    private static readonly Color ColHigh = new Color(0.8f, 0.15f, 0.15f);

    public void Setup(PacketData p, System.Action onAck)
    {
        idText.text = p.id;
        uidText.text = "uid:" + p.userId;
        encText.text = p.encryption == EncryptionType.AES256 ? "AES-256"
                     : p.encryption == EncryptionType.DES ? "DES" : "—";

        riskText.text = p.riskLevel.ToString().ToUpper();
        riskBg.color = p.riskLevel == RiskLevel.Low ? ColLow
                      : p.riskLevel == RiskLevel.High ? ColHigh : ColMed;

        ackButton.onClick.AddListener(() => onAck.Invoke());
    }
}