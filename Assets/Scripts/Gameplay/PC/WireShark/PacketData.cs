// PacketData.cs
// Representa um nico pacote de rede no jogo

using System;

[Serializable]
public class PacketData
{
    public string PacketId;         // ex: "PKT-0041"
    public string ConversationId;   // ex: "CONV-192-10"  (entre dois IPs)
    public string SrcIP;
    public string DstIP;
    public string Protocol;         // "TCP" ou "UDP"
    public string EncryptionType;   // "AES" ou "DES"
    public string EncryptedPayload; // o texto encriptado (hex string)
    public string PlainText;        // o texto original  s usado internamente para ScriptableObject
    public string Hash;             // hash fictcio gerado
    public int MessageIndex;     // ndice na conversa (1, 2, 3...)
    public bool IsImportant;      // marca os pacotes com intel relevante
    public float Timestamp;        // tempo de jogo em que apareceu
}