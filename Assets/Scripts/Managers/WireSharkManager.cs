using System.Collections.Generic;
using UnityEngine;

// #my_code - Simulação do Wireshark: pacotes como GameObjects, captura e exibição na UI
public class WiresharkManager : MonoBehaviour
{
    private WiresharkUI ui;

    // pacotes visíveis no stream ao vivo - inseridos no início para simular a chegada de novos pacotes no topo da lista
    private List<PacketData> livePackets = new List<PacketData>();

    // histório organizado por conversaId -> lista de pacotes - construído pelo PacketGenerator no Start
    private Dictionary<string, List<PacketData>> history = new Dictionary<string, List<PacketData>>();

    private PacketData selectedPacket = null;

    // limite do stream ao vivo - manter 50 pacotes é suficiente para a ilusão de atividade sem pesar na memória
    private const int MaxLivePackets = 50;

    void Awake()
    {
        ui = GetComponent<WiresharkUI>();
    }

    // chamado pelo PacketGenerator quando um novo pacote chega - insere no início para que os mais recentes fiquem no topo
    public void ReceivePacket(PacketData pkt)
    {
        livePackets.Insert(0, pkt);

        if (livePackets.Count > MaxLivePackets)
            livePackets.RemoveAt(livePackets.Count - 1);

        ui.AddPacketRow(pkt);
    }

    // chamado pelo PacketGenerator no Start com o histórico pré-construído - passa ao WiresharkUI para popular o painel de histórico
    public void SetHistory(Dictionary<string, List<PacketData>> hist)
    {
        history = hist;
        ui.RefreshHistory(history);
    }

    // chamado pelo PacketRowUI quando o jogador clica numa linha - atualiza o painel de detalhe
    public void SelectPacket(PacketData pkt)
    {
        selectedPacket = pkt;
        ui.ShowPacketDetail(pkt);
    }

    // copia o pacote selecionado para o GameClipboard para que o jogador o possa colar no TerminalManager -
    // GameClipboard é um estático que persiste entre cenas sem MonoBehaviour
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

    // chamado pelo botão PEDIR ACK numa conversa do histórico -
    // aceder a dados históricos de outras conversas é suspeito, por isso aumenta a suspeita temporariamente -
    // o nível aleatório entre 1 e 2 representa que o sistema pode ou não notar o acesso
    public List<PacketData> RequestHistoryConversation(string conversationId)
    {
        if (!history.ContainsKey(conversationId))
            return new List<PacketData>();

        float suspicionLevel = Random.Range(1f, 2f);
        SuspicionManager.Instance.IncreaseSuspicion(suspicionLevel, GetInstanceID(), SuspicionManager.SuspicionSource.TerminalAccess);

        // o ACK é um evento pontual - após 2 segundos, a fonte de suspeita é removida e o decay começa
        StartCoroutine(StopSuspicionAfterDelay(2f));

        Debug.Log($"[WiresharkManager] ACK pedido para {conversationId} - suspeita aumentada.");

        return history[conversationId];
    }

    private System.Collections.IEnumerator StopSuspicionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());
    }
}