// WiresharkManager.cs
// Lgica principal da app Wireshark
// Adiciona ao mesmo GameObject que o WiresharkUI e o PacketGenerator

using System.Collections.Generic;
using UnityEngine;

// #my_code - Simulação do Wireshark: pacotes como GameObjects, captura e exibição na UI
public class WiresharkManager : MonoBehaviour
{
    private WiresharkUI ui;

    // pacotes ao vivo (stream atual)
    private List<PacketData> livePackets = new List<PacketData>();

    // histrico: conversaId -> lista de pacotes
    private Dictionary<string, List<PacketData>> history = new Dictionary<string, List<PacketData>>();

    // pacote atualmente selecionado
    private PacketData selectedPacket = null;

    // mximo de pacotes visveis no stream ao vivo
    private const int MaxLivePackets = 50;

    void Awake()
    {
        ui = GetComponent<WiresharkUI>();
    }

    // chamado pelo PacketGenerator quando um novo pacote chega
    public void ReceivePacket(PacketData pkt)
    {
        livePackets.Insert(0, pkt);

        // limita o histrico vivo
        if (livePackets.Count > MaxLivePackets)
            livePackets.RemoveAt(livePackets.Count - 1);

        ui.AddPacketRow(pkt);
    }

    // chamado pelo PacketGenerator no Start com o histrico pr-construdo
    public void SetHistory(Dictionary<string, List<PacketData>> hist)
    {
        history = hist;
        ui.RefreshHistory(history);
    }

    // chamado pelo PacketRowUI quando o jogador clica numa linha
    public void SelectPacket(PacketData pkt)
    {
        selectedPacket = pkt;
        ui.ShowPacketDetail(pkt);
    }

    // chamado pelo boto COPIAR PACOTE
    public void CopySelectedPacket()
    {
        if (selectedPacket == null)
        {
            ui.ShowCopyStatus("nenhum pacote selecionado.");
            return;
        }

        GameClipboard.Copy(selectedPacket.PacketId, selectedPacket.EncryptedPayload, selectedPacket.EncryptionType);
        ui.ShowCopyStatus("copiado: " + selectedPacket.PacketId);

        Debug.Log($"[WiresharkManager] Pacote {selectedPacket.PacketId} copiado para GameClipboard.");
    }

    // chamado pelo boto PEDIR ACK numa conversa do histrico
    // devolve todos os pacotes dessa conversa e aplica suspeita
    public List<PacketData> RequestHistoryConversation(string conversationId)
    {
        if (!history.ContainsKey(conversationId))
            return new List<PacketData>();

        // aplica suspeita  usa o SuspicionManager existente
        float suspicionLevel = Random.Range(1f, 2f);
        SuspicionManager.Instance.IncreaseSuspicion(suspicionLevel, GetInstanceID(), SuspicionManager.SuspicionSource.TerminalAccess);

        // para a suspeita aps um pequeno delay  o ACK  um evento pontual
        StartCoroutine(StopSuspicionAfterDelay(2f));

        Debug.Log($"[WiresharkManager] ACK pedido para {conversationId}  suspeita aumentada.");

        return history[conversationId];
    }

    private System.Collections.IEnumerator StopSuspicionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());
    }
}