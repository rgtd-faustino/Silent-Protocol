// CryptoHelper.cs
// Encriptao e desencriptao AES e DES real usando System.Security.Cryptography
// Usado pelo PacketGenerator para encriptar e pelo TerminalManager para desencriptar

using System;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    // chaves fixas do jogo  o jogador no as v, so internas
    // AES precisa de 16, 24 ou 32 bytes
    private static readonly byte[] AES_KEY = Encoding.UTF8.GetBytes("SilentProtocol16");
    private static readonly byte[] AES_IV = Encoding.UTF8.GetBytes("InitVector123456");

    // DES precisa exatamente de 8 bytes
    private static readonly byte[] DES_KEY = Encoding.UTF8.GetBytes("SecKey8B");
    private static readonly byte[] DES_IV = Encoding.UTF8.GetBytes("IVBytes8");

    // ---------------------------------------------------------------
    // AES
    // ---------------------------------------------------------------

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

                // devolve como hex string separada por espaos (estilo Wireshark)
                return BytesToHexString(outputBytes);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro AES Encrypt: " + e.Message);
            return "";
        }
    }

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

    // ---------------------------------------------------------------
    // DES
    // ---------------------------------------------------------------

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

    // ---------------------------------------------------------------
    // Utilitrios de converso hex
    // ---------------------------------------------------------------

    // converte bytes para string hex separada por espaos: "48 65 6c 6c 6f"
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

    // converte string hex (com ou sem espaos) de volta para bytes
    public static byte[] HexStringToBytes(string hexString)
    {
        string clean = hexString.Replace(" ", "").Trim();
        byte[] bytes = new byte[clean.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
        return bytes;
    }

    // gera um hash fictcio mas consistente (primeiros 6 chars do SHA1 do texto)
    public static string GenerateHash(string text)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hash).Replace("-", "").Substring(0, 6).ToLower();
        }
    }

    // mtodo de convenincia: encripta conforme o tipo
    public static string Encrypt(string plainText, string encType)
    {
        return encType == "DES" ? EncryptDES(plainText) : EncryptAES(plainText);
    }

    // mtodo de convenincia: desencripta conforme o tipo
    public static string Decrypt(string hexPayload, string encType)
    {
        return encType == "DES" ? DecryptDES(hexPayload) : DecryptAES(hexPayload);
    }
}