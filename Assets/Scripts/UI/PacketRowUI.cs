using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PacketRowUI : MonoBehaviour
{
    public TextMeshProUGUI idText, srcText, dstText, protoText, encText;
    public Button rowButton;

    private static readonly Color ColDNS = new Color(0.8f, 0.27f, 0.8f);
    private static readonly Color ColTCP = new Color(0.27f, 0.67f, 0.8f);
    private static readonly Color ColUDP = new Color(0.27f, 0.67f, 1.0f);
    private static readonly Color ColAES = new Color(0.53f, 1.0f, 0.67f);
    private static readonly Color ColDES = new Color(1.0f, 0.8f, 0.27f);
    private static readonly Color ColNone = new Color(0.2f, 0.4f, 0.2f);

    public void Setup(PacketData p, System.Action onClick)
    {
        idText.text = p.id;
        srcText.text = p.srcIP;
        dstText.text = p.dstIP;
        protoText.text = p.proto.ToString();
        protoText.color = p.proto == ProtoType.DNS ? ColDNS
                        : p.proto == ProtoType.TCP ? ColTCP : ColUDP;

        if (p.encryption == EncryptionType.AES256)
        { encText.text = "e... AES-256"; encText.color = ColAES; }
        else if (p.encryption == EncryptionType.DES)
        { encText.text = "e... DES"; encText.color = ColDES; }
        else
        { encText.text = "r... —"; encText.color = ColNone; }

        rowButton.onClick.AddListener(() => onClick.Invoke());
    }
}