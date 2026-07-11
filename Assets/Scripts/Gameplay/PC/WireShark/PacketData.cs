using System;

[Serializable]
public class PacketData
{
    public string PacketId;
    public string ConversationId;
    public string SrcIP;
    public string DstIP;
    public string Protocol;
    public string EncryptionType;
    
    // a string em formato hexadecimal visualizada na UI da app de rede
    public string EncryptedPayload;
    
    // conserva a mensagem limpa internamente para validações do sistema e entrega de intel sem ter de forçar desencriptações redudantes no código
    public string PlainText;
    public string Hash;
    
    // posição ordenada na troca de mensagens para sabermos construir a árvore da conversa na interface
    public int MessageIndex;
    public bool IsImportant;
    public float Timestamp;
}