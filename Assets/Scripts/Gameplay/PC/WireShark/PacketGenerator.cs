using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketGenerator : MonoBehaviour
{
    [Header("Horários de pacotes (um NetworkSchedule asset por dia, por ordem)")]
    [SerializeField] private NetworkSchedule[] daySchedules;

    [Header("App Wireshark (para detetar se está aberta)")]
    [SerializeField] private GameObject wiresharkAppObject;

    [Header("Pacotes de ruído (fundo)")]
    [SerializeField] private float minNoiseInterval = 1.5f;
    [SerializeField] private float maxNoiseInterval = 4f;
    
    [Tooltip("Define se queremos injetar tráfego falso intermitente para dar mais vida à stream de dados.")]
    [SerializeField] private bool generateNoise = true;

    // conjuntos de IPs e strings de rotina configurados à mão para injetar algum ambiente na rede
    private static readonly string[] NoiseIPs = {
        "192.168.1.10", "192.168.1.22", "10.0.0.5", "10.0.0.44",
        "172.16.0.8", "172.16.0.15", "8.8.8.8", "1.1.1.1"
    };
    private static readonly string[] Protocols = { "TCP", "UDP", "DNS", "HTTP" };
    private static readonly string[] NoiseEncTypes = { "AES", "AES", "DES" };
    private static readonly string[] NoisePhrases = {
        "session keepalive", "auth token refresh", "heartbeat ping",
        "data sync request", "cache invalidation", "user activity log",
        "telemetry packet", "connection handshake"
    };

    private WiresharkManager manager;
    
    // usado como contador progressivo para assegurar que cada pacote tem o seu identificador distinto
    private int packetCounter = 0;

    // guarda e agrupa os pacotes enviados consoante o identificador da conversa para mapear o histórico completo entregue ao WiresharkManager
    private Dictionary<string, List<PacketData>> historyConversations = new Dictionary<string, List<PacketData>>();

    // registo transitório dos pacotes já agendados mas cujo momento no jogo ainda não chegou
    private List<ScheduledPacket> pendingPackets = new List<ScheduledPacket>();

    void Awake()
    {
        manager = GetComponent<WiresharkManager>();
    }

    void Start()
    {
        LoadDaySchedule();

        if (generateNoise)
            StartCoroutine(NoiseLoop());

        StartCoroutine(ScheduledPacketLoop());
    }

    // carrega o planeamento consoante o dia reportado pelo GameManager
    // atribui os pacotes antigos diretamente ao histórico e empurra o resto para a fila de espera
    private void LoadDaySchedule()
    {
        NetworkSchedule schedule = GetScheduleForToday();

        if (schedule == null || schedule.packets == null || schedule.packets.Count == 0)
        {
            Debug.LogWarning($"[PacketGenerator] Sem NetworkSchedule para o dia {GameManager.Instance.currentDay}.");
            manager.SetHistory(historyConversations);
            return;
        }

        float nowMinutes = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

        foreach (ScheduledPacket sp in schedule.packets)
        {
            float spawnMinutes = sp.spawnHour * 60f;

            if (spawnMinutes <= nowMinutes)
            {
                AddToHistory(sp);
            }
            else
            {
                pendingPackets.Add(sp);
            }
        }

        manager.SetHistory(historyConversations);

        Debug.Log($"[PacketGenerator] Dia {GameManager.Instance.currentDay}: " +
                  $"{historyConversations.Count} conversa(s) no histórico, " +
                  $"{pendingPackets.Count} pacote(s) agendado(s).");
    }

    // corotina encarregada de descarregar os pacotes no exato momento certo
    // encaminha os dados para a stream live se a UI estiver visível ou sorrateiramente para o histórico se não estiver
    private IEnumerator ScheduledPacketLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (pendingPackets.Count == 0) continue;

            float nowMinutes = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

            for (int i = pendingPackets.Count - 1; i >= 0; i--)
            {
                ScheduledPacket sp = pendingPackets[i];

                if (nowMinutes < sp.spawnHour * 60f) continue;

                pendingPackets.RemoveAt(i);

                bool appOpen = wiresharkAppObject != null && wiresharkAppObject.activeInHierarchy;

                if (appOpen)
                {
                    PacketData pkt = BuildPacket(sp);
                    manager.ReceivePacket(pkt);
                    Debug.Log($"[PacketGenerator] Pacote '{sp.conversationId}' enviado ao vivo.");
                }
                else
                {
                    AddToHistory(sp);
                    manager.SetHistory(historyConversations);
                    Debug.Log($"[PacketGenerator] Pacote '{sp.conversationId}' adicionado ao histórico (app fechada).");
                }
            }
        }
    }

    // engorda a stream com pacotes irrelevantes gerados periodicamente
    // estes pacotes não ficam guardados em lado nenhum se a app estiver fechada
    private IEnumerator NoiseLoop()
    {
        while (true)
        {
            float wait = Random.Range(minNoiseInterval, maxNoiseInterval);
            yield return new WaitForSeconds(wait);

            bool appOpen = wiresharkAppObject != null && wiresharkAppObject.activeInHierarchy;
            if (!appOpen) continue;

            manager.ReceivePacket(GenerateNoisePacket());
        }
    }

    // instancia os objetos de transmissão final
    // comunica com o CryptoHelper para ofuscar o texto se o pacote tiver a tag adequada no scriptable object
    private PacketData BuildPacket(ScheduledPacket sp)
    {
        packetCounter++;

        string encTypeName = sp.encryptionType.ToString();

        string payload = sp.isEncrypted
            ? CryptoHelper.Encrypt(sp.plainText, encTypeName)
            : sp.plainText;

        string displayEncType = sp.isEncrypted ? encTypeName : "NONE";

        return new PacketData
        {
            PacketId = "PKT-" + packetCounter.ToString("D4"),
            ConversationId = sp.conversationId,
            SrcIP = sp.srcIP,
            DstIP = sp.dstIP,
            Protocol = sp.protocol,
            EncryptionType = displayEncType,
            EncryptedPayload = payload,
            PlainText = sp.plainText,
            Hash = CryptoHelper.GenerateHash(sp.plainText),
            MessageIndex = sp.messageIndex,
            IsImportant = sp.isImportant,
            Timestamp = Time.time
        };
    }

    // anexa um pacote ao seu grupo conversacional na coleção baseada em dicionário
    private void AddToHistory(ScheduledPacket sp)
    {
        PacketData pkt = BuildPacket(sp);

        if (!historyConversations.ContainsKey(sp.conversationId))
            historyConversations[sp.conversationId] = new List<PacketData>();

        historyConversations[sp.conversationId].Add(pkt);
    }

    // compõe pacotes fictícios juntando peças de matrizes estáticas criadas ali em cima
    private PacketData GenerateNoisePacket()
    {
        packetCounter++;
        string encType = NoiseEncTypes[Random.Range(0, NoiseEncTypes.Length)];
        string phrase = NoisePhrases[Random.Range(0, NoisePhrases.Length)];
        string src = NoiseIPs[Random.Range(0, NoiseIPs.Length)];
        string dst;
        do { dst = NoiseIPs[Random.Range(0, NoiseIPs.Length)]; } while (dst == src);

        return new PacketData
        {
            PacketId = "PKT-" + packetCounter.ToString("D4"),
            ConversationId = "NOISE-" + src.Split('.')[3] + "-" + dst.Split('.')[3],
            SrcIP = src,
            DstIP = dst,
            Protocol = Protocols[Random.Range(0, Protocols.Length)],
            EncryptionType = encType,
            EncryptedPayload = CryptoHelper.Encrypt(phrase, encType),
            PlainText = phrase,
            Hash = CryptoHelper.GenerateHash(phrase),
            MessageIndex = 1,
            IsImportant = false,
            Timestamp = Time.time
        };
    }

    // apanha o plano de emissões certo para hoje consumindo o estado gerido globalmente
    private NetworkSchedule GetScheduleForToday()
    {
        int index = GameManager.Instance.currentDay - 1;
        if (daySchedules == null || index < 0 || index >= daySchedules.Length) return null;
        return daySchedules[index];
    }
}