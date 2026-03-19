using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmailButton : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    //public TextMeshProUGUI senderText;
    private EmailData emailData;
    private EmailManager manager;

    public void Setup(EmailData data, EmailManager mgr)
    {
        emailData = data;
        manager = mgr;

        // Se tiver só 1 TMP Text
        titleText.text = $"{data.sender} | {data.title}";
    }

    public void OnClick()
    {
        manager.OpenEmail(emailData);
    }
}
