using System;
using System.Security.Cryptography;
using System.Text;

// #my_code - Integração de bibliotecas de encriptação reais para desencriptação in-game
public static class CryptoHelper
{
    // Chaves hardcoded usadas na ofuscação de rede
    // Fixas porque não temos necessidade de gerar chaves dinamicamente para o jogador adivinhar
    private static readonly byte[] AES_KEY = Encoding.UTF8.GetBytes("SilentProtocol16");
    private static readonly byte[] AES_IV = Encoding.UTF8.GetBytes("InitVector123456");

    private static readonly byte[] DES_KEY = Encoding.UTF8.GetBytes("SecKey8B");
    private static readonly byte[] DES_IV = Encoding.UTF8.GetBytes("IVBytes8");

    // Usa a implementação de AES da biblioteca nativa para simular payloads encriptados reais
    // O PacketGenerator chama isto antes de injetar os pacotes na interface
    public static string EncryptAES(string plainText)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AES_KEY;
                aes.IV = AES_IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] outputBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return BytesToHexString(outputBytes);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro AES Encrypt: " + e.Message);
            return "";
        }
    }

    // Processo inverso acionado pelo TerminalManager quando o jogador submete um comando de descodificação
    public static string DecryptAES(string hexPayload)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AES_KEY;
                aes.IV = AES_IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] inputBytes = HexStringToBytes(hexPayload);
                ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] outputBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Encoding.UTF8.GetString(outputBytes);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro AES Decrypt: " + e.Message);
            return "";
        }
    }

    // Variante da encriptação com DES para dar variedade visual aos pacotes que passam
    public static string EncryptDES(string plainText)
    {
        try
        {
            using (DES des = DES.Create())
            {
                des.Key = DES_KEY;
                des.IV = DES_IV;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = des.CreateEncryptor();
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] outputBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return BytesToHexString(outputBytes);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro DES Encrypt: " + e.Message);
            return "";
        }
    }

    public static string DecryptDES(string hexPayload)
    {
        try
        {
            using (DES des = DES.Create())
            {
                des.Key = DES_KEY;
                des.IV = DES_IV;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;

                byte[] inputBytes = HexStringToBytes(hexPayload);
                ICryptoTransform decryptor = des.CreateDecryptor();
                byte[] outputBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Encoding.UTF8.GetString(outputBytes);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro DES Decrypt: " + e.Message);
            return "";
        }
    }

    // Formata o array de bytes numa string hexadecimal espaçada para simular o aspeto dos pacotes reais em ferramentas de rede
    public static string BytesToHexString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(bytes[i].ToString("x2"));
        }
        return sb.ToString();
    }

    // Retira os espaços visuais e devolve o array limpo para as funções de desencriptação poderem processar os blocos
    public static byte[] HexStringToBytes(string hexString)
    {
        string clean = hexString.Replace(" ", "").Trim();
        byte[] bytes = new byte[clean.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
        return bytes;
    }

    // Gera um identificador rápido usando um bocado do hash SHA1
    // Ajuda a dar a aparência de uma assinatura digital na interface sem sobrecarregar o sistema com lógicas complexas
    public static string GenerateHash(string text)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hash).Replace("-", "").Substring(0, 6).ToLower();
        }
    }

    // Wrapper simples para encaminhar a ofuscação pelo algoritmo correspondente ao pacote
    public static string Encrypt(string plainText, string encType)
    {
        return encType == "DES" ? EncryptDES(plainText) : EncryptAES(plainText);
    }

    // Wrapper para devolver o texto limpo com base na escolha do Terminal
    public static string Decrypt(string hexPayload, string encType)
    {
        return encType == "DES" ? DecryptDES(hexPayload) : DecryptAES(hexPayload);
    }
}