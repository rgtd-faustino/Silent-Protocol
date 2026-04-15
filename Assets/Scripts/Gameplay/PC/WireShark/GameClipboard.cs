// GameClipboard.cs
// Faz a ponte entre o WiresharkApp e o Terminal

public static class GameClipboard
{
    public static string PacketId { get; private set; } = "";
    public static string PacketContent { get; private set; } = "";
    public static string EncryptionType { get; private set; } = ""; // "AES" ou "DES"
    public static bool HasContent => !string.IsNullOrEmpty(PacketContent);

    public static void Copy(string id, string content, string encType)
    {
        PacketId = id;
        PacketContent = content;
        EncryptionType = encType;
    }

    public static void Clear()
    {
        PacketId = "";
        PacketContent = "";
        EncryptionType = "";
    }
}