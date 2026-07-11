using System;
using System.Security.Cryptography;
using System.Text;

// #my_code - Integração de bibliotecas de encriptação reais para desencriptação in-game
public static class CryptoHelper {
    // chaves hardcoded usadas na ofuscação de rede, são fixas porque não temos necessidade de gerar chaves dinamicamente para o jogador adivinhar
    // AES-128 exige uma chave de exatamente 16 bytes (128 bits), por isso o "SilentProtocol16" tem 16 caracteres
    // se a string tivesse um tamanho diferente de 16/24/32 bytes, o Aes.Create() lançaria uma exceção ao atribuir aes.Key
    private static readonly byte[] AES_KEY = Encoding.UTF8.GetBytes("SilentProtocol16");

    // o IV (Initialization Vector) do AES tem sempre o tamanho do bloco do algoritmo, que é 16 bytes independentemente do tamanho da chave
    // é por isso que "InitVector123456" também tem exatamente 16 caracteres
    // está fixo (em vez de gerado aleatoriamente por bloco) porque só precisamos que o resultado seja reproduzível para o puzzle do terminal
    private static readonly byte[] AES_IV = Encoding.UTF8.GetBytes("InitVector123456");

    // DES usa chaves de 8 bytes (64 bits, dos quais só 56 são efetivamente usados), daí "SecKey8B" ter 8 caracteres
    // escolhemos DES aqui apenas por ser visualmente/estruturalmente diferente do AES (payload mais curto, hex diferente)
    // até porque é um algoritmo considerado inseguro para uso real
    private static readonly byte[] DES_KEY = Encoding.UTF8.GetBytes("SecKey8B");

    // o bloco do DES é de 8 bytes, logo o IV também tem de ter 8 bytes e "IVBytes8" cumpre esse requisito
    private static readonly byte[] DES_IV = Encoding.UTF8.GetBytes("IVBytes8");

    // usa a implementação de AES da biblioteca nativa para simular payloads encriptados reais
    // o PacketGenerator chama isto antes de injetar os pacotes na interface para aqueles que precisem de ser encriptados (para não serem vistos em texto simples)
    public static string EncryptAES(string plainText) {
        try {
            using Aes aes = Aes.Create();
            aes.Key = AES_KEY;
            aes.IV = AES_IV;
            // CBC (Cipher Block Chaining) foi escolhido porque exige um IV e encadeia blocos entre si, o que produz um output com aspeto mais "real"
            // (blocos diferentes mesmo com texto repetido) do que, por exemplo, ECB, que deixaria padrões repetidos visíveis e pouco convincentes no terminal
            aes.Mode = CipherMode.CBC;

            // PKCS7 preenche o último bloco até ao tamanho fixo do bloco (16 bytes no AES) sempre que o texto
            // original não é múltiplo exato desse tamanho. escolhemos este por ser o padrão mais comum e por remover o padding de forma automática
            aes.Padding = PaddingMode.PKCS7;

            // CreateEncryptor() usa a Key/IV já atribuídas acima, tem de ser exatamente as mesmas usadas depois
            // no CreateDecryptor(), senão o resultado da desencriptação sai corrompido/ilegível
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);

            // TransformFinalBlock processa o array completo de uma vez porque os payloads dos pacotes são strings curtas então não há necessidade de processar em pedaços
            byte[] outputBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return BytesToHexString(outputBytes);

        } catch (Exception e) {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro AES Encrypt: " + e.Message);
            return "";
        }
    }

    // processo inverso do EncryptAES, chamado pelo TerminalManager quando o jogador tenta descodificar um payload no terminal
    public static string DecryptAES(string hexPayload) {
        try {
            using Aes aes = Aes.Create();
            // Key e IV têm de ser idênticos aos usados no EncryptAES, é a mesma chave simétrica dos dois lados, por isso não se geram novos valores aqui
            aes.Key = AES_KEY;
            aes.IV = AES_IV;
            // modo e padding têm de corresponder exatamente aos usados na encriptação
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] inputBytes = HexStringToBytes(hexPayload);
            ICryptoTransform decryptor = aes.CreateDecryptor();
            // tal como na encriptação, processamos tudo de uma vez porque os payloads são pequenos
            byte[] outputBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return Encoding.UTF8.GetString(outputBytes);

        } catch (Exception e) {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro AES Decrypt: " + e.Message);
            return "";
        }
    }

    // variante em DES só para dar variedade visual aos pacotes, o jogador nunca precisa de saber a diferença entre os dois algoritmos
    public static string EncryptDES(string plainText) {
        try {
            using DES des = DES.Create();
            des.Key = DES_KEY;
            des.IV = DES_IV;
            // usamos CBC pela mesma razão que no AES, ou seja, encadear blocos evita padrões repetidos óbvios
            // e mantém a mesma "lógica" de configuração entre os dois algoritmos
            des.Mode = CipherMode.CBC;
            // PKCS7 novamente por ser o standard e por o bloco do DES (8 bytes) também precisar de padding sempre que o texto não é múltiplo de 8 bytes
            des.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = des.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] outputBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return BytesToHexString(outputBytes);

        } catch (Exception e) {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro DES Encrypt: " + e.Message);
            return "";
        }
    }

    // processo inverso do EncryptDES
    public static string DecryptDES(string hexPayload) {
        try {
            using DES des = DES.Create();
            // mesma Key/IV fixas do EncryptDES, obrigatório para que a desencriptação simétrica funcione
            des.Key = DES_KEY;
            des.IV = DES_IV;
            // mesmo Mode/Padding do EncryptDES, pelo mesmo motivo do par AES: têm de coincidir exatamente com o que foi usado a encriptar
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;

            byte[] inputBytes = HexStringToBytes(hexPayload);
            ICryptoTransform decryptor = des.CreateDecryptor();
            byte[] outputBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return Encoding.UTF8.GetString(outputBytes);

        } catch (Exception e) {
            UnityEngine.Debug.LogError("[CryptoHelper] Erro DES Decrypt: " + e.Message);
            return "";
        }
    }

    // formata os bytes em hexadecimal com espaços entre cada um, só para parecer visualmente um pacote de rede real
    // o espaço a cada 2 caracteres hex (1 byte) foi escolhido só por legibilidade no terminal do jogo para imitar o aspeto de dumps hexadecimais reais tipo o Wireshark
    public static string BytesToHexString(byte[] bytes) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++) {
            if (i > 0) sb.Append(' ');
            sb.Append(bytes[i].ToString("x2")); // "x2" garante sempre 2 dígitos hex minúsculos por byte (ex: "0a", não "a")
        }
        return sb.ToString();
    }

    // operação inversa do BytesToHexString, tira os espaços para os bytes poderem ser processados pela desencriptação
    public static byte[] HexStringToBytes(string hexString) {
        // remove os espaços introduzidos pelo BytesToHexString antes de reconstruir os bytes já que Convert.ToByte espera pares de caracteres hex seguidos ("a1 3f 09" -> "a13f09")
        string clean = hexString.Replace(" ", "").Trim();
        byte[] bytes = new byte[clean.Length / 2]; // cada byte corresponde a 2 caracteres hex

        for (int i = 0; i < bytes.Length; i++)
            // substring(i*2, 2) extrai o par de caracteres correspondente a cada byte
            // base 16 (hexadecimal) porque é o formato em que os bytes foram escritos por BytesToHexString
            bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
        return bytes;
    }

    // só para dar aparência de assinatura digital na UI, cortamos o hash SHA1 para os primeiros 6 caracteres para decoração
    // SHA1 foi escolhido (em vez de SHA256, por exemplo) só porque produz um hash mais curto e é mais que suficiente para gerar uma "assinatura" com aspeto plausível
    public static string GenerateHash(string text) {
        using SHA1 sha1 = SHA1.Create();
        byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
        // substring(0, 6) corta para 6 caracteres hex (3 bytes) só por estética
        return BitConverter.ToString(hash).Replace("-", "").Substring(0, 6).ToLower();
    }

    // só para o PacketGenerator não ter de saber qual dos dois algoritmos usar, escolhe com base na string que vem definida no pacote
    // comparação simples por string ("DES" vs qualquer outro valor, assumido como AES) porque só existem estes dois algoritmos suportados no jogo
    public static string Encrypt(string plainText, string encType) {
        return encType == "DES" ? EncryptDES(plainText) : EncryptAES(plainText);
    }

    // mesma ideia mas para o lado do Terminal quando o jogador tenta descodificar tem de usar exatamente o mesmo encType que foi usado para encriptar o payload correspondente,
    // caso contrário tenta desencriptar com o algoritmo errado e o resultado sai ilegível
    public static string Decrypt(string hexPayload, string encType) {
        return encType == "DES" ? DecryptDES(hexPayload) : DecryptAES(hexPayload);
    }
}