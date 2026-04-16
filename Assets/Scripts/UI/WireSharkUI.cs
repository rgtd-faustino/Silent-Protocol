// WiresharkUI.cs
// Controla todo o UI da app Wireshark
// Adiciona ao mesmo GameObject que WiresharkManager e PacketGenerator

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WiresharkUI : MonoBehaviour
{
    [Header("Stream ao vivo")]
    [SerializeField] private ScrollRect packetScroll;
    [SerializeField] private Transform packetContent;
    [SerializeField] private GameObject packetRowPrefab;
    [SerializeField] private TextMeshProUGUI txtPacketCount;

    [Header("Detalhe do pacote selecionado")]
    [SerializeField] private TextMeshProUGUI txtSelectedId;
    [SerializeField] private TextMeshProUGUI txtDetailSrcMAC;
    [SerializeField] private TextMeshProUGUI txtDetailSrcIP;
    [SerializeField] private TextMeshProUGUI txtDetailDstIP;
    [SerializeField] private TextMeshProUGUI txtDetailProtocol;
    [SerializeField] private TextMeshProUGUI txtDetailEncType;
    [SerializeField] private TextMeshProUGUI txtDetailHash;
    [SerializeField] private TextMeshProUGUI txtDetailConvId;
    [SerializeField] private TextMeshProUGUI txtDetailMsgIndex;
    [SerializeField] private TextMeshProUGUI txtHexPayload;
    [SerializeField] private TextMeshProUGUI txtCopyStatus;
    [SerializeField] private Button btnCopyPacket;

    [Header("Histórico")]
    [SerializeField] private ScrollRect historyScroll;
    [SerializeField] private Transform historyContent;
    [SerializeField] private GameObject historyRowPrefab;

    [Header("Popup Histórico (conversa completa)")]
    [SerializeField] private GameObject historyPopup;
    [SerializeField] private TextMeshProUGUI txtPopupTitle;
    [SerializeField] private Transform popupContent;
    [SerializeField] private GameObject popupRowPrefab; // linha simples de texto para o popup
    [SerializeField] private Button btnClosePopup;

    [Header("VPN Bar")]
    [SerializeField] private TextMeshProUGUI txtVpnStatus;

    private WiresharkManager manager;
    private int packetCount = 0;
    private List<PacketRowUI> activeRows = new List<PacketRowUI>();

    void Awake()
    {
        manager = GetComponent<WiresharkManager>();
    }

    void Start()
    {
        btnCopyPacket.onClick.AddListener(() => manager.CopySelectedPacket());

        if (btnClosePopup != null)
            btnClosePopup.onClick.AddListener(() => historyPopup.SetActive(false));

        if (historyPopup != null)
            historyPopup.SetActive(false);

        UpdateVpnStatus();
    }

    // ---------------------------------------------------------------
    // Stream ao vivo
    // ---------------------------------------------------------------

    public void AddPacketRow(PacketData pkt)
    {
        packetCount++;

        // instancia a linha no topo do content
        GameObject obj = Instantiate(packetRowPrefab, packetContent);
        obj.transform.SetAsFirstSibling();

        PacketRowUI row = obj.GetComponent<PacketRowUI>();
        row.Setup(pkt, manager);
        activeRows.Insert(0, row);

        // limita o número de linhas visíveis
        if (activeRows.Count > 50)
        {
            Destroy(activeRows[activeRows.Count - 1].gameObject);
            activeRows.RemoveAt(activeRows.Count - 1);
        }

        if (txtPacketCount != null)
            txtPacketCount.text = packetCount + " pacotes";

        StartCoroutine(ScrollToTop());
    }

    // ---------------------------------------------------------------
    // Detalhe do pacote
    // ---------------------------------------------------------------

    public void ShowPacketDetail(PacketData pkt)
    {
        // desseleciona todas as linhas
        foreach (var row in activeRows)
            row.SetSelected(false);

        if (txtSelectedId != null) txtSelectedId.text = pkt.PacketId + "  —  " + pkt.ConversationId;
        if (txtDetailSrcIP != null) txtDetailSrcIP.text = pkt.SrcIP;
        if (txtDetailDstIP != null) txtDetailDstIP.text = pkt.DstIP;
        if (txtDetailProtocol != null) txtDetailProtocol.text = pkt.Protocol;
        if (txtDetailEncType != null) txtDetailEncType.text = pkt.EncryptionType;
        if (txtDetailHash != null) txtDetailHash.text = pkt.Hash;
        if (txtDetailConvId != null) txtDetailConvId.text = pkt.ConversationId;
        if (txtDetailMsgIndex != null) txtDetailMsgIndex.text = "msg " + pkt.MessageIndex;

        // MAC fictício baseado no IP para consistência
        if (txtDetailSrcMAC != null) txtDetailSrcMAC.text = GenerateFakeMAC(pkt.SrcIP);

        // payload hex — trunca se for muito longo para o UI
        if (txtHexPayload != null)
        {
            string hex = pkt.EncryptedPayload;
            txtHexPayload.text = hex.Length > 120 ? hex.Substring(0, 120) + "..." : hex;
        }

        if (txtCopyStatus != null)
            txtCopyStatus.text = "";
    }

    public void ShowCopyStatus(string msg)
    {
        if (txtCopyStatus != null)
            txtCopyStatus.text = msg;

        StartCoroutine(ClearCopyStatusAfter(3f));
    }

    // ---------------------------------------------------------------
    // Histórico
    // ---------------------------------------------------------------

    public void RefreshHistory(Dictionary<string, List<PacketData>> hist)
    {
        Debug.Log($"[RefreshHistory] hist.Count={hist.Count}, historyContent={historyContent}");
        if (historyContent == null) { Debug.LogError("[RefreshHistory] historyContent é null!"); return; }
        foreach (Transform child in historyContent)
            Destroy(child.gameObject);

        foreach (var kvp in hist)
        {
            GameObject obj = Instantiate(historyRowPrefab, historyContent);
            HistoryRowUI row = obj.GetComponent<HistoryRowUI>();
            row.Setup(kvp.Key, kvp.Value, manager, this);
        }
    }

    // mostra o popup com todos os pacotes de uma conversa do histórico
    public void ShowHistoryConversation(string convId, List<PacketData> packets)
    {
        if (historyPopup == null) return;

        historyPopup.SetActive(true);

        if (txtPopupTitle != null)
            txtPopupTitle.text = "conversa: " + convId;

        foreach (Transform child in popupContent)
            Destroy(child.gameObject);

        foreach (var pkt in packets)
        {
            GameObject obj = Instantiate(popupRowPrefab, popupContent);
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = "[" + pkt.MessageIndex + "] " + pkt.PacketId
                         + "  " + pkt.SrcIP + " → " + pkt.DstIP
                         + "  [" + pkt.EncryptionType + "]";
        }
    }

    // ---------------------------------------------------------------
    // VPN Bar
    // ---------------------------------------------------------------

    private void UpdateVpnStatus()
    {
        // placeholder — a lógica de inteligência vai aqui depois
        if (txtVpnStatus != null)
            txtVpnStatus.text = "REDE PRIVADA — bloqueada (requer 100pts inteligência)";
    }

    // ---------------------------------------------------------------
    // Utilitários
    // ---------------------------------------------------------------

    private IEnumerator ScrollToTop()
    {
        yield return null;
        yield return null;
        packetScroll.verticalNormalizedPosition = 1f;
    }

    private IEnumerator ClearCopyStatusAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (txtCopyStatus != null)
            txtCopyStatus.text = "";
    }

    private string GenerateFakeMAC(string ip)
    {
        // gera sempre o mesmo MAC para o mesmo IP (determinístico)
        int hash = ip.GetHashCode();
        byte[] bytes = System.BitConverter.GetBytes(hash);
        return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:3E:7B",
            bytes[0], bytes[1], bytes[2], bytes[3]);
    }
}