using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EmailManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inboxPanel;
    public GameObject emailViewPanel;
    public GameObject emailButtonPrefab;   // arrasta o prefab aqui
    public Transform inboxContentParent;    // um panel vazio dentro do InboxPanel para colocar os botões
    private List<EmailData> inboxEmails = new List<EmailData>();

    [Header("Email Content UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI senderText;
    public TextMeshProUGUI bodyText;


    public void OpenEmail(EmailData email)
    {
        inboxPanel.SetActive(false);
        emailViewPanel.SetActive(true);

        titleText.text = email.title;
        senderText.text = email.sender;
        bodyText.text = email.body;


    }
    void Start()
    {
        TestReceiveEmail();
    }



    public void BackToInbox()
    {
        emailViewPanel.SetActive(false);
        inboxPanel.SetActive(true);
    }
    public void ReceiveEmail(EmailData email)
    {
        inboxEmails.Add(email);

        // criar botão
        GameObject newButton = Instantiate(emailButtonPrefab, inboxContentParent);
        newButton.SetActive(true);

        EmailButton btnScript = newButton.GetComponent<EmailButton>();
        btnScript.Setup(email, this);  // envia os dados do email + manager

    }
    public void TestReceiveEmail()
    {
        EmailData testEmail = new EmailData();
        testEmail.title = "Olá João!";
        testEmail.sender = "Sistema";
        testEmail.body = "Isto é um email de teste.";

        ReceiveEmail(testEmail); // chama o método que instancia o botão
        Debug.Log("Email recebido: " + testEmail.title);
    }




}
