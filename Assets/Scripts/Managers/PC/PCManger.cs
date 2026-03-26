using UnityEngine;

public class PCManager : MonoBehaviour
{
    public void OpenWindow(GameObject window)
    {
        window.SetActive(true);
        window.transform.SetAsLastSibling(); // traz para frente
    }
}