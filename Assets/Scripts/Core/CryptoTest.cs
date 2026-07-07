using UnityEngine;

public class CryptoTest : MonoBehaviour
{
    void Start()
    {
        string original = "acesso concedido ao servidor";

        string encrypted = CryptoHelper.EncryptAES(original);
        Debug.Log("ENCRYPTED: " + encrypted);

        string decrypted = CryptoHelper.DecryptAES(encrypted);
        Debug.Log("DECRYPTED: " + decrypted);
    }
}