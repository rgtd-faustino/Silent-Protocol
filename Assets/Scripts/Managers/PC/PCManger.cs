using UnityEngine;

public class PCManager : MonoBehaviour
{
    // abre uma janela de app no PC e mete-a para a frente na UI
    // o SetAsLastSibling garante que a janela ativa fica sempre por cima das outras no canvas
    public void OpenWindow(GameObject window)
    {
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.singKeyboardSound);
        window.SetActive(true);
        window.transform.SetAsLastSibling();
    }
}