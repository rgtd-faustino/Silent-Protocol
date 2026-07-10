using UnityEngine;

public class Window : MonoBehaviour
{
    // Usado pelos botões de fechar nas várias janelas do UI do PC
    // Apenas desativa o GameObject para manter o seu estado caso o jogador volte a abrir a janela
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
