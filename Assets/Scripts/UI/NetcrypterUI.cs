using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetcrypterUI : MonoBehaviour
{
    [Header("Toolbar")]
    public Button btnLive, btnPause, btnFilter, btnExport;
    public TextMeshProUGUI ifaceLabel;

    [Header("Status Bar")]
    public TextMeshProUGUI intelPtsText;
    public TextMeshProUGUI suspicionPctText;
    public Image suspicionBar;      // fill image (0..1)
    public TextMeshProUGUI privateNetStatus;
    public TextMeshProUGUI privateNetTimer;
    public Button btnPrivateNet;

    [Header("Packet List")]
    public Transform packetListContent;
    public GameObject packetRowPrefab;   // tem PacketRowUI
    public TMP_InputField filterInput;
    public Button btnApplyFilter;
    public TextMeshProUGUI packetCountLabel;

    [Header("Packet Detail")]
    public TextMeshProUGUI detailSelected;
    public TextMeshProUGUI detSrcMAC, detDstMAC;
    public TextMeshProUGUI detSrcIP, detDstIP, detTTL, detSize;
    public TextMeshProUGUI detEnc, detHash, detUID;
    public Button btnCopy;
    public GameObject copyFlashPanel;   // popup "copiado"
    public TextMeshProUGUI copyFlashText;

    [Header("History")]
    public GameObject historyPanel;
    public Transform historyContent;
    public GameObject historyRowPrefab;  // tem HistoryRowUI

    [Header("Message Popup")]
    public GameObject msgPanel;
    public TextMeshProUGUI msgText;

    private NetcrypterManager _mgr;

    void Start()
    {
        _mgr = GetComponent<NetcrypterManager>();

        btnApplyFilter.onClick.AddListener(() =>
            _mgr.ApplyFilter(filterInput.text));

        btnCopy.onClick.AddListener(() => _mgr.CopySelected());

        btnPrivateNet.onClick.AddListener(() => _mgr.TryActivatePrivateNet());

        copyFlashPanel.SetActive(false);
        historyPanel.SetActive(false);
        msgPanel.SetActive(false);
        SetCopyButtonEnabled(false);
    }

    // ── Lista de pacotes ─────────────────────────────────────

    public void RenderPacketList(List<PacketData> packets)
    {
        foreach (Transform child in packetListContent)
            Destroy(child.gameObject);

        foreach (var p in packets)
        {
            var go = Instantiate(packetRowPrefab, packetListContent);
            var row = go.GetComponent<PacketRowUI>();
            row.Setup(p, () => _mgr.SelectPacket(p));
        }
        packetCountLabel.text = packets.Count + " pacotes";
    }

    // ── Detalhe ──────────────────────────────────────────────

    public void ShowPacketDetail(PacketData p)
    {
        detailSelected.text = "pacote " + p.id;
        detSrcMAC.text = p.srcMAC; detDstMAC.text = p.dstMAC;
        detSrcIP.text = p.srcIP; detDstIP.text = p.dstIP;
        detTTL.text = p.ttl.ToString();
        detSize.text = p.sizeBytes + " B";
        detEnc.text = p.encryption == EncryptionType.None
                         ? "nenhuma" : p.encryption.ToString();
        detHash.text = p.hash;
        detUID.text = p.userId;
    }

    public void SetCopyButtonEnabled(bool on)
    {
        btnCopy.interactable = on;
    }

    public void ShowCopyFlash(string packetId)
    {
        copyFlashText.text = "pacote " + packetId + " copiado";
        StartCoroutine(FlashCoroutine(copyFlashPanel, 1.8f));
    }

    // ── Histórico ─────────────────────────────────────────────

    public void ShowHistoryPanel(List<PacketData> packets)
    {
        historyPanel.SetActive(true);
        foreach (Transform child in historyContent)
            Destroy(child.gameObject);

        foreach (var p in packets)
        {
            var go = Instantiate(historyRowPrefab, historyContent);
            var row = go.GetComponent<HistoryRowUI>();
            row.Setup(p, () => _mgr.RequestACK(p));
        }
    }

    public void HideHistoryPanel() => historyPanel.SetActive(false);

    public void ShowACKResult(string id, bool detected)
    {
        string msg = detected
            ? $"⚠ ACK {id} — DETETADO! suspeita aumentou."
            : $"ACK {id} recebido sem incidentes.";
        ShowMessage(msg);
    }

    // ── Status bar ────────────────────────────────────────────

    public void RefreshStatus(int pts, float sus, bool netActive)
    {
        intelPtsText.text = "INT: " + pts + "pts";
        suspicionBar.fillAmount = sus;
        suspicionPctText.text = Mathf.RoundToInt(sus * 100) + "%";
        privateNetStatus.text = netActive
            ? "REDE PRIVADA ativa"
            : "REDE PRIVADA bloqueada";
    }

    public void UpdatePrivateNetTimer(float t)
    {
        privateNetTimer.text = Mathf.CeilToInt(t) + "s";
    }

    // ── Utilitários ───────────────────────────────────────────

    public void ShowMessage(string msg)
    {
        msgText.text = msg;
        StartCoroutine(FlashCoroutine(msgPanel, 2.5f));
    }

    private IEnumerator FlashCoroutine(GameObject panel, float duration)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(duration);
        panel.SetActive(false);
    }
}