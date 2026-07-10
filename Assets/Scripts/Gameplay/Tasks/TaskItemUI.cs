using TMPro;
using UnityEngine;

public class TaskItemUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI label;

    private string taskName;
    private string deadline;

    // O TaskManager invoca isto quando a task sai da queue e fica visível ao jogador. A string do deadline já vem bem formatada de lá.
    public void SetTask(string name, string deadlineStr) {
        taskName = name;
        deadline = deadlineStr;
        Render(false, false);
        gameObject.SetActive(true);
    }

    public void SetCompleted() {
        Render(true, false);
    }
    public void SetFailed() {
        Render(false, true);
    }

    // Usamos as tags de rich text do TextMeshPro para alternar estados visualmente sem ter de instanciar objetos diferentes. Completo fica apenas riscado, falhado fica a vermelho.
    private void Render(bool completed, bool failed) {
        string text = $"{taskName}  <size=75%><color=#AAAAAA>{deadline}</color></size>";

        if (completed)
            label.text = $"<s>{text}</s>";
        else if (failed)
            label.text = $"<color=#FF6060><s>{text}</s></color>";
        else
            label.text = text;
    }
}