using UnityEngine;

public class GameClipboard : MonoBehaviour
{
    public static GameClipboard Instance;

    private string _content = "";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Wireshark chama isto ao clicar "Copiar Pacote"
    public void Copy(string content)
    {
        _content = content;
        Debug.Log("[Clipboard] Copiado: " + content);
    }

    // Terminal chama isto ao clicar "Colar"
    public string Paste()
    {
        return _content;
    }

    public bool HasContent() => !string.IsNullOrEmpty(_content);
}