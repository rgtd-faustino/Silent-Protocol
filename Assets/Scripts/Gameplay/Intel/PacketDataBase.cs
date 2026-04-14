using System.Collections.Generic;
using UnityEngine;

public class PacketDatabase : MonoBehaviour
{
    public static PacketDatabase Instance;

    public List<PacketData> LivePackets = new();
    public List<PacketData> HistoryPackets = new();

    private static readonly string[] _ips = {
        "172.16.0.35", "172.16.0.27", "10.0.0.46",
        "192.168.1.199", "192.168.1.131", "10.0.0.19"
    };
    private static readonly string[] _users = {
        "USR_884", "USR_221", "USR_046", "USR_131"
    };

    void Awake()
    {
        Instance = this;
        GeneratePackets();
    }

    void GeneratePackets()
    {
        // Gera 20 pacotes live
        for (int i = 0; i < 20; i++)
            LivePackets.Add(MakePacket(80 - i, false));

        // Gera 10 pacotes no histórico
        for (int i = 0; i < 10; i++)
            HistoryPackets.Add(MakePacket(30 - i, true));
    }

    private PacketData MakePacket(int idNum, bool isHistory)
    {
        var enc = (EncryptionType)Random.Range(0, 3);
        var uid = _users[Random.Range(0, _users.Length)];
        var hash = GenHash();

        return new PacketData
        {
            id = "#" + idNum.ToString("D4"),
            srcIP = _ips[Random.Range(0, _ips.Length)],
            dstIP = "192.168.1.104",
            srcMAC = GenMAC(),
            dstMAC = "FF:FF:FF:FF:FF:FF",
            proto = (ProtoType)Random.Range(0, 3),
            encryption = enc,
            userId = uid,
            hash = enc == EncryptionType.None ? "—" : hash,
            ttl = Random.Range(0, 2) == 0 ? 64 : 128,
            sizeBytes = Random.Range(48, 512),
            isHistory = isHistory,
            riskLevel = (RiskLevel)Random.Range(0, 3),
            rawPayload = BuildPayload(enc, uid, hash)
        };
    }

    // Monta o texto que o terminal vai receber e tentar decriptar
    private string BuildPayload(EncryptionType enc, string uid, string hash)
    {
        return enc switch
        {
            EncryptionType.AES256 =>
                $"AES256_PKT_uid:{uid}_hash:{hash}_data:{GenHex()}",
            EncryptionType.DES =>
                $"DES_PKT_uid:{uid}_hash:{hash}_data:{GenHex()}",
            _ =>
                $"RAW_PKT_uid:{uid}_data:{GenHex()}"
        };
    }

    private string GenHash()
    {
        const string c = "abcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 6; i++) sb.Append(c[Random.Range(0, c.Length)]);
        return sb.ToString();
    }

    private string GenMAC()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 6; i++)
        {
            if (i > 0) sb.Append(':');
            sb.Append(Random.Range(0, 256).ToString("X2"));
        }
        return sb.ToString();
    }

    private string GenHex()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 12; i++)
            sb.Append(Random.Range(0, 256).ToString("X2"));
        return sb.ToString();
    }
}