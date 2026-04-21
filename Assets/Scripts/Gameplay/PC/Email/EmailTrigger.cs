using UnityEngine;

/// <summary>
/// Exemplo de uso: coloca este script num trigger/objeto do mundo.
/// Quando o jogador interage ou entra na zona, o email é enviado.
///
/// No Inspector:
///  - emailParaEnviar → arrasta o EmailItem asset
/// </summary>
public class EmailTrigger : MonoBehaviour
{
    [SerializeField] private EmailItem emailParaEnviar;

    // Chama isto quando quiseres enviar o email (trigger, evento de missão, etc.)
    public void EnviarEmail()
    {
        if (emailParaEnviar != null)
            EmailManager.Instance.ReceberEmail(emailParaEnviar);
    }

    // Exemplo: enviar ao entrar no trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            EnviarEmail();
    }
}