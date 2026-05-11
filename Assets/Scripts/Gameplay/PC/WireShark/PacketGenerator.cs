// PacketGenerator.cs
// Lê o NetworkSchedule do dia e envia os pacotes nas horas certas.
// Se a app Wireshark estiver aberta quando o pacote chega -> aparece no stream ao vivo.
// Se estiver fechada -> vai direto para o histórico.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketGenerator : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Horários por dia — daySchedules[0] = Dia 1, etc.
    // ---------------------------------------------------------------
    [Header("Horários de pacotes (um NetworkSchedule asset por dia, por ordem)")]
    [SerializeField] private NetworkSchedule[] daySchedules;

    // ---------------------------------------------------------------
    // Referência ao canvas/GameObject da app Wireshark
    // Arrasta aqui o GameObject que é ativado/desativado quando o jogador abre a app
    // O PacketGenerator usa isto para saber se a app está aberta ou fechada
    // ---------------------------------------------------------------
    [Header("App Wireshark (para detetar se está aberta)")]
    [SerializeField] private GameObject wiresharkAppObject;

    [Header("Pacotes de ruído (fundo)")]
    [SerializeField] private float minNoiseInterval = 1.5f;
    [SerializeField] private float maxNoiseInterval = 4f;
    [Tooltip("Quantos pacotes de ruído aparecem por minuto de jogo (aproximado)")]
    [SerializeField] private bool generateNoise = true;

    // IPs e protocolos para pacotes de ruído
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
    private int packetCounter = 0;

    // histórico acumulado: conversationId -> lista de pacotes
    private Dictionary<string, List<PacketData>> historyConversations = new Dictionary<string, List<PacketData>>();

    // pacotes agendados que ainda não foram enviados
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

    // ---------------------------------------------------------------
    // Carrega o schedule do dia atual e separa:
    //   - pacotes cuja hora já passou -> histórico imediato
    //   - pacotes cuja hora ainda não chegou -> fila pendente
    // ---------------------------------------------------------------
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
                // hora já passou — vai direto para o histórico
                AddToHistory(sp);
            }
            else
            {
                // ainda não chegou a hora — coloca na fila
                pendingPackets.Add(sp);
            }
        }

        // envia o histórico inicial para o manager
        manager.SetHistory(historyConversations);

        Debug.Log($"[PacketGenerator] Dia {GameManager.Instance.currentDay}: " +
                  $"{historyConversations.Count} conversa(s) no histórico, " +
                  $"{pendingPackets.Count} pacote(s) agendado(s).");
    }

    // ---------------------------------------------------------------
    // Loop que verifica a cada frame se chegou a hora de algum pacote agendado
    // ---------------------------------------------------------------
    private IEnumerator ScheduledPacketLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f); // verifica duas vezes por segundo

            if (pendingPackets.Count == 0) continue;

            float nowMinutes = TimeManager.Instance.GetCurrentTimeInHours() * 60f;

            // usa índice inverso para poder remover durante a iteração
            for (int i = pendingPackets.Count - 1; i >= 0; i--)
            {
                ScheduledPacket sp = pendingPackets[i];

                if (nowMinutes < sp.spawnHour * 60f) continue;

                pendingPackets.RemoveAt(i);

                bool appOpen = wiresharkAppObject != null && wiresharkAppObject.activeInHierarchy;

                if (appOpen)
                {
                    // app está aberta -> aparece no stream ao vivo
                    PacketData pkt = BuildPacket(sp);
                    manager.ReceivePacket(pkt);
                    Debug.Log($"[PacketGenerator] Pacote '{sp.conversationId}' enviado ao vivo.");
                }
                else
                {
                    // app está fechada -> vai para o histórico
                    AddToHistory(sp);
                    manager.SetHistory(historyConversations); // atualiza o histórico no manager
                    Debug.Log($"[PacketGenerator] Pacote '{sp.conversationId}' adicionado ao histórico (app fechada).");
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // Loop de ruído de fundo — pacotes aleatórios para dar vida ao stream
    // ---------------------------------------------------------------
    private IEnumerator NoiseLoop()
    {
        while (true)
        {
            float wait = Random.Range(minNoiseInterval, maxNoiseInterval);
            yield return new WaitForSeconds(wait);

            // ruído só aparece no stream ao vivo se a app estiver aberta
            bool appOpen = wiresharkAppObject != null && wiresharkAppObject.activeInHierarchy;
            if (!appOpen) continue;

            manager.ReceivePacket(GenerateNoisePacket());
        }
    }

    // ---------------------------------------------------------------
    // Constrói um PacketData a partir de um ScheduledPacket
    // ---------------------------------------------------------------
    private PacketData BuildPacket(ScheduledPacket sp)
    {
        packetCounter++;

        string encTypeName = sp.encryptionType.ToString(); // "AES" ou "DES"

        // se não for encriptado, o payload mostra o texto em claro
        string payload = sp.isEncrypted
            ? CryptoHelper.Encrypt(sp.plainText, encTypeName)
            : sp.plainText;

        // se não for encriptado, o encryptionType mostra "NONE" na UI
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

    // ---------------------------------------------------------------
    // Adiciona um pacote ao dicionário de histórico
    // ---------------------------------------------------------------
    private void AddToHistory(ScheduledPacket sp)
    {
        PacketData pkt = BuildPacket(sp);

        if (!historyConversations.ContainsKey(sp.conversationId))
            historyConversations[sp.conversationId] = new List<PacketData>();

        historyConversations[sp.conversationId].Add(pkt);
    }

    // ---------------------------------------------------------------
    // Ruído aleatório de fundo
    // ---------------------------------------------------------------
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

    // ---------------------------------------------------------------
    // Seleciona o NetworkSchedule do dia atual
    // ---------------------------------------------------------------
    private NetworkSchedule GetScheduleForToday()
    {
        int index = GameManager.Instance.currentDay - 1;
        if (daySchedules == null || index < 0 || index >= daySchedules.Length) return null;
        return daySchedules[index];
    }
}