using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetcrypterManager : MonoBehaviour
{
    [Header("Configuração")]
    public float ackDetectChance = 0.4f;   // 40% chance de ser apanhado
    public float suspicionPerDetect = 0.12f;  // +12% suspeita por deteção
    public float privateNetDuration = 60f;    // segundos de rede privada base
    public int privateNetMinPts = 100;    // pontos para desbloquear

    [Header("Estado")]
    public float suspicion = 0f;    // 0..1
    public int intelPoints = 47;
    public bool privateNetActive = false;
    private float _privateNetTimer = 0f;

    private NetcrypterUI _ui;
    private PacketData _selected;
    private List<PacketData> _filtered = new();

    void Start()
    {
        _ui = GetComponent<NetcrypterUI>();
        ApplyFilter("");
        _ui.RefreshStatus(intelPoints, suspicion, privateNetActive);
    }

    void Update()
    {
        if (privateNetActive)
        {
            _privateNetTimer -= Time.deltaTime;
            _ui.UpdatePrivateNetTimer(_privateNetTimer);
            if (_privateNetTimer <= 0f) DeactivatePrivateNet();
        }
    }

    // ── Filtro ──────────────────────────────────────────────

    public void ApplyFilter(string filterText)
    {
        _filtered.Clear();
        string f = filterText.ToLower().Trim();

        foreach (var p in PacketDatabase.Instance.LivePackets)
        {
            if (string.IsNullOrEmpty(f) ||
                p.srcIP.Contains(f) ||
                p.proto.ToString().ToLower().Contains(f) ||
                p.userId.ToLower().Contains(f))
            {
                _filtered.Add(p);
            }
        }
        _ui.RenderPacketList(_filtered);
    }

    // ── Seleção ──────────────────────────────────────────────

    public void SelectPacket(PacketData p)
    {
        _selected = p;
        _ui.ShowPacketDetail(p);
        _ui.SetCopyButtonEnabled(true);
    }

    public void CopySelected()
    {
        if (_selected == null) return;
        GameClipboard.Instance.Copy(_selected.rawPayload);
        _ui.ShowCopyFlash(_selected.id);
    }

    // ── ACK (histórico) ──────────────────────────────────────

    public void RequestACK(PacketData histPacket)
    {
        bool detected = Random.value < ackDetectChance;

        if (detected)
        {
            suspicion = Mathf.Clamp01(suspicion + suspicionPerDetect);
            _ui.RefreshStatus(intelPoints, suspicion, privateNetActive);
            _ui.ShowACKResult(histPacket.id, true);
        }
        else
        {
            _ui.ShowACKResult(histPacket.id, false);
        }

        // Mostra o detalhe do pacote histórico na janela direita
        SelectPacket(histPacket);
    }

    // ── Rede Privada ─────────────────────────────────────────

    public void TryActivatePrivateNet()
    {
        if (intelPoints < privateNetMinPts)
        {
            _ui.ShowMessage($"Precisas de {privateNetMinPts}pts. Tens {intelPoints}pts.");
            return;
        }
        // Duração escala proporcionalmente com os pontos acima do mínimo
        float bonus = (intelPoints - privateNetMinPts) * 0.5f;
        _privateNetTimer = privateNetDuration + bonus;
        privateNetActive = true;
        _ui.ShowHistoryPanel(PacketDatabase.Instance.HistoryPackets);
        _ui.RefreshStatus(intelPoints, suspicion, privateNetActive);
    }

    private void DeactivatePrivateNet()
    {
        privateNetActive = false;
        _ui.HideHistoryPanel();
        _ui.RefreshStatus(intelPoints, suspicion, privateNetActive);
    }

    // Chamado pelo IntelManager quando guardas intel
    public void AddIntelPoints(int pts)
    {
        intelPoints += pts;
        _ui.RefreshStatus(intelPoints, suspicion, privateNetActive);
    }
}