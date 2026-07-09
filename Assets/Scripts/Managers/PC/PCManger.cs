using UnityEngine;

public class PCManager : MonoBehaviour
{
    public void OpenWindow(GameObject window)
    {
        // som de clique ao abrir uma app no PC
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.singKeyboardSound);
        window.SetActive(true);
        window.transform.SetAsLastSibling(); // traz para frente
    }
}