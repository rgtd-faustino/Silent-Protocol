// HistoryRowUI.cs
// Script do prefab HistoryRowPrefab
// Cada conversa no histórico tem este script

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtConvId;
    [SerializeField] private TextMeshProUGUI txtInfo;
    [SerializeField] private TextMeshProUGUI txtRisk;
    [SerializeField] private Button btnAck;

    private static readonly Color ColLowRisk = new Color(0.16f, 0.55f, 0.16f);
    private static readonly Color ColMedRisk = new Color(0.55f, 0.43f, 0.16f);
    private static readonly Color ColHighRisk = new Color(0.55f, 0.16f, 0.16f);

    private string conversationId;
    private WiresharkManager manager;
    private WiresharkUI ui;

    public void Setup(string convId, List<PacketData> packets, WiresharkManager mgr, WiresharkUI wiresharkUI)
    {
        conversationId = convId;
        manager = mgr;
        ui = wiresharkUI;

        txtConvId.text = convId;

        if (packets.Count > 0)
        {
            var first = packets[0];
            txtInfo.text = first.SrcIP + " ↔ " + first.DstIP + "  [" + packets.Count + " msgs]";

            // risco baseado na suspeita atual — podia ser baseado noutros fatores no futuro
            float risk = SuspicionManager.Instance.GetSuspicionRatio();
            if (risk < 0.33f)
            {
                txtRisk.text = "BAIXO";
                txtRisk.color = ColLowRisk;
            }
            else if (risk < 0.66f)
            {
                txtRisk.text = "MÉDIO";
                txtRisk.color = ColMedRisk;
            }
            else
            {
                txtRisk.text = "ALTO";
                txtRisk.color = ColHighRisk;
            }
        }

        btnAck.onClick.AddListener(OnAckPressed);
    }

    private void OnAckPressed()
    {
        List<PacketData> packets = manager.RequestHistoryConversation(conversationId);

        if (packets == null || packets.Count == 0)
        {
            Debug.Log("[HistoryRowUI] Sem pacotes para esta conversa.");
            return;
        }

        // mostra os pacotes da conversa num popup ou painel lateral
        ui.ShowHistoryConversation(conversationId, packets);
    }
}