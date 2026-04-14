public enum EncryptionType { None, AES256, DES }
public enum ProtoType { DNS, TCP, UDP }
public enum RiskLevel { Low, Medium, High }

[System.Serializable]
public class PacketData
{
    public string id;
    public string srcIP;
    public string dstIP;
    public string srcMAC;
    public string dstMAC;
    public ProtoType proto;
    public EncryptionType encryption;
    public string userId;
    public string hash;
    public int ttl;
    public int sizeBytes;
    public bool isHistory;     // veio do histórico?
    public RiskLevel riskLevel;

    // Payload que vai para o terminal — gerado com base na encriptação
    public string rawPayload;
}