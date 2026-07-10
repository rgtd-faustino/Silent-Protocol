public static class GameClipboard
{
    public static string PacketId { get; private set; } = "";
    
    // Armazena temporariamente a payload copiada pelo jogador na interface
    // A ligação é feita assim para que o Terminal consiga aceder ao conteúdo sem conhecer as referências diretas à app de rede
    public static string PacketContent { get; private set; } = "";
    
    // Essencial para o Terminal saber que algoritmo aplicar na desencriptação do que foi copiado
    public static string EncryptionType { get; private set; } = "";
    
    public static bool HasContent => !string.IsNullOrEmpty(PacketContent);

    // Invocado pelos botões de copiar da interface de pacotes
    public static void Copy(string id, string content, string encType)
    {
        PacketId = id;
        PacketContent = content;
        EncryptionType = encType;
    }

    // Usado nos resets de final de dia para garantir que o jogador não leva lixo retido em memória para a frente
    public static void Clear()
    {
        PacketId = "";
        PacketContent = "";
        EncryptionType = "";
    }
}