using UnityEngine;

public class Window : MonoBehaviour
{
    // usado pelos botões de fechar nas várias janelas da UI do PC
    // apenas desativa o GameObject para manter o seu estado caso o jogador volte a abrir a janela
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
