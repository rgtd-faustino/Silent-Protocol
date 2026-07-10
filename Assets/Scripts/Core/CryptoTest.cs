using UnityEngine;

public class CryptoTest : MonoBehaviour
{
    // usamos isto só para confirmar que o AES está a funcionar antes de meter nos emails, para não estragar o flow do jogo
    void Start()
    {
        string original = "acesso concedido ao servidor";

        string encrypted = CryptoHelper.EncryptAES(original);
        Debug.Log("ENCRYPTED: " + encrypted);

        string decrypted = CryptoHelper.DecryptAES(encrypted);
        Debug.Log("DECRYPTED: " + decrypted);
    }
}