// PacketGenerator.cs
// Gera o stream de pacotes — mistura ruído aleatório com conversas predefinidas
// Adiciona ao mesmo GameObject que o WiresharkManager

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketGenerator : MonoBehaviour
{
    [Header("Conversas Predefinidas")]
    [SerializeField] private ConversationData conversationData;

    [Header("Geração")]
    [SerializeField] private float minInterval = 1.5f;
    [SerializeField] private float maxInterval = 4f;
    [Tooltip("Quantos pacotes aleatórios aparecem entre cada pacote importante")]
    [SerializeField] private int noisePacketsBetweenImportant = 3;

    // IPs e protocolos para pacotes de ruído
    private static readonly string[] NoiseIPs = {
        "192.168.1.10", "192.168.1.22", "10.0.0.5", "10.0.0.44",
        "172.16.0.8", "172.16.0.15", "8.8.8.8", "1.1.1.1"
    };
    private static readonly string[] Protocols = { "TCP", "UDP", "DNS", "HTTP" };
    private static readonly string[] EncTypes = { "AES", "AES", "DES" };

    // frases de ruído — serão encriptadas
    private static readonly string[] NoisePhrases = {
        "session keepalive",
        "auth token refresh",
        "heartbeat ping",
        "data sync request",
        "cache invalidation",
        "user activity log",
        "telemetry packet",
        "connection handshake"
    };

    private WiresharkManager manager;
    private int packetCounter = 0;
    private int noiseCount = 0;

    // fila de pacotes importantes prontos a ser injetados
    private Queue<PacketData> importantQueue = new Queue<PacketData>();

    // histórico: conversas cujos pacotes "já passaram" (appearsLive = false)
    private Dictionary<string, List<PacketData>> historyConversations = new Dictionary<string, List<PacketData>>();

    void Awake()
    {
        manager = GetComponent<WiresharkManager>();
    }

    void Start()
    {
        if (manager == null)
            Debug.LogError("[PacketGenerator] WiresharkManager não encontrado! Mesmo GameObject?");
    
        BuildImportantQueue();
        BuildHistory();
        StartCoroutine(GenerateLoop());
    }

    // pré-encripta todos os pacotes importantes e mete-os na fila
    private void BuildImportantQueue()
    {
        if (conversationData == null)
        {
            Debug.LogError("conversationData NULL");
            return;
        }

        if (conversationData.conversations == null)
        {
            Debug.LogError("conversationData.conversations NULL");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager NULL");
            return;
        }

        foreach (var conv in conversationData.conversations)
        {
            if (conv.messages == null)
            {
                Debug.LogError("messages NULL na conversa: " + conv.conversationId);
                continue;
            }

            // (conv.dayToAppear != GameManager.Instance.CurrentDay) continue;
            // TEMPORÁRIO — substitui as duas condições de filtro por:
            if (conv.dayToAppear > 1) continue;

            for (int i = 0; i < conv.messages.Count; i++)
            {
                var msg = conv.messages[i];
                if (!msg.appearsLive) continue;

                PacketData pkt = BuildPacketFromMessage(conv, msg, i + 1);
                importantQueue.Enqueue(pkt);
            }
        }
    }


    // constrói o histórico com pacotes que já passaram (appearsLive = false)
    private void BuildHistory()
    {
        if (conversationData == null) return;

        foreach (var conv in conversationData.conversations)
        {
            //if (conv.dayToAppear > GameManager.Instance.CurrentDay) continue;
            // TEMPORÁRIO — substitui as duas condições de filtro por:
            if (conv.dayToAppear > 1) continue;


            List<PacketData> histList = new List<PacketData>();

            for (int i = 0; i < conv.messages.Count; i++)
            {
                var msg = conv.messages[i];
                if (msg.appearsLive) continue;

                PacketData pkt = BuildPacketFromMessage(conv, msg, i + 1);
                histList.Add(pkt);
            }

            if (histList.Count > 0)
            {
                if (!historyConversations.ContainsKey(conv.conversationId))
                    historyConversations[conv.conversationId] = new List<PacketData>();

                historyConversations[conv.conversationId].AddRange(histList);
            }
        }

        // envia o histórico para o manager
        manager.SetHistory(historyConversations);
        Debug.Log($"[PacketGenerator] {historyConversations.Count} conversas no histórico.");
    }

    // loop principal de geração
    private IEnumerator GenerateLoop()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            PacketData pkt;

            // injeta um pacote importante a cada N pacotes de ruído
            if (importantQueue.Count > 0 && noiseCount >= noisePacketsBetweenImportant)
            {
                pkt = importantQueue.Dequeue();
                noiseCount = 0;
            }
            else
            {
                pkt = GenerateNoisePacket();
                noiseCount++;
            }

            manager.ReceivePacket(pkt);
        }
    }

    // gera um pacote de ruído aleatório
    private PacketData GenerateNoisePacket()
    {
        packetCounter++;
        string encType = EncTypes[Random.Range(0, EncTypes.Length)];
        string phrase = NoisePhrases[Random.Range(0, NoisePhrases.Length)];
        string src = NoiseIPs[Random.Range(0, NoiseIPs.Length)];
        string dst = NoiseIPs[Random.Range(0, NoiseIPs.Length)];

        while (dst == src)
            dst = NoiseIPs[Random.Range(0, NoiseIPs.Length)];

        string encrypted = CryptoHelper.Encrypt(phrase, encType);
        string convId = "NOISE-" + src.Split('.')[3] + "-" + dst.Split('.')[3];

        return new PacketData
        {
            PacketId = "PKT-" + packetCounter.ToString("D4"),
            ConversationId = convId,
            SrcIP = src,
            DstIP = dst,
            Protocol = Protocols[Random.Range(0, Protocols.Length)],
            EncryptionType = encType,
            EncryptedPayload = encrypted,
            PlainText = phrase,
            Hash = CryptoHelper.GenerateHash(phrase),
            MessageIndex = 1,
            IsImportant = false,
            Timestamp = Time.time
        };
    }

    // constrói um PacketData a partir de um ConversationEntry + MessageEntry
    private PacketData BuildPacketFromMessage(ConversationEntry conv, MessageEntry msg, int index)
    {
        packetCounter++;
        string encrypted = CryptoHelper.Encrypt(msg.plainText, conv.encryptionType);

        return new PacketData
        {
            PacketId = "PKT-" + packetCounter.ToString("D4"),
            ConversationId = conv.conversationId,
            SrcIP = index % 2 == 1 ? conv.srcIP : conv.dstIP,
            DstIP = index % 2 == 1 ? conv.dstIP : conv.srcIP,
            Protocol = conv.protocol,
            EncryptionType = conv.encryptionType,
            EncryptedPayload = encrypted,
            PlainText = msg.plainText,
            Hash = CryptoHelper.GenerateHash(msg.plainText),
            MessageIndex = index,
            IsImportant = msg.isImportant,
            Timestamp = Time.time
        };
    }
}