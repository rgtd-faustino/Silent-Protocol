// PacketRowUI.cs
// Script do prefab PacketRowPrefab
// Cada linha do stream ao vivo tem este script

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PacketRowUI : MonoBehaviour
{
    [Header("Textos da linha")]
    [SerializeField] private TextMeshProUGUI txtPacketId;
    [SerializeField] private TextMeshProUGUI txtSrcIP;
    [SerializeField] private TextMeshProUGUI txtDstIP;
    [SerializeField] private TextMeshProUGUI txtProtocol;
    [SerializeField] private TextMeshProUGUI txtInfo;
    [SerializeField] private TextMeshProUGUI txtEncType;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image importantIndicator; // ponto verde lateral para pacotes importantes

    // cores
    private static readonly Color ColNormal = new Color(0.05f, 0.10f, 0.05f, 0f);
    private static readonly Color ColSelected = new Color(0.10f, 0.22f, 0.10f, 1f);
    private static readonly Color ColImportant = new Color(0.08f, 0.18f, 0.08f, 1f);
    private static readonly Color ColIdText = new Color(0.16f, 0.37f, 0.16f);
    private static readonly Color ColSrcText = new Color(0.16f, 0.43f, 0.43f);
    private static readonly Color ColDstText = new Color(0.43f, 0.43f, 0.16f);
    private static readonly Color ColProtoText = new Color(0.43f, 0.16f, 0.43f);
    private static readonly Color ColInfoText = new Color(0.23f, 0.48f, 0.23f);
    private static readonly Color ColAES = new Color(0.16f, 0.43f, 0.16f);
    private static readonly Color ColDES = new Color(0.43f, 0.29f, 0.16f);

    private PacketData data;
    private WiresharkManager manager;
    private bool isSelected = false;

    public void Setup(PacketData pkt, WiresharkManager mgr)
    {
        data = pkt;
        manager = mgr;

        txtPacketId.text = pkt.PacketId;
        txtSrcIP.text = pkt.SrcIP;
        txtDstIP.text = pkt.DstIP;
        txtProtocol.text = pkt.Protocol;
        txtInfo.text = pkt.IsImportant
                                ? "!! " + TruncateIP(pkt.SrcIP) + " → " + TruncateIP(pkt.DstIP)
                                : TruncateIP(pkt.SrcIP) + " → " + TruncateIP(pkt.DstIP);
        txtEncType.text = pkt.EncryptionType;

        txtPacketId.color = ColIdText;
        txtSrcIP.color = ColSrcText;
        txtDstIP.color = ColDstText;
        txtProtocol.color = ColProtoText;
        txtInfo.color = ColInfoText;
        txtEncType.color = pkt.EncryptionType == "DES" ? ColDES : ColAES;

        if (importantIndicator != null)
            importantIndicator.gameObject.SetActive(pkt.IsImportant);

        SetSelected(false);
    }

    public void OnClick()
    {
        manager?.SelectPacket(data);
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (backgroundImage == null) return;

        if (selected)
            backgroundImage.color = ColSelected;
        else if (data != null && data.IsImportant)
            backgroundImage.color = ColImportant;
        else
            backgroundImage.color = ColNormal;
    }

    private string TruncateIP(string ip)
    {
        // mostra só os últimos dois octetos: 192.168.1.10 -> 1.10
        string[] parts = ip.Split('.');
        return parts.Length >= 2 ? parts[^2] + "." + parts[^1] : ip;
    }
}