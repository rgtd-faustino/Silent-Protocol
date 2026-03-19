using UnityEngine;

public class Window : MonoBehaviour
{
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
