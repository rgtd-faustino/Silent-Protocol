using System.Collections.Generic;
using UnityEngine;

public class PCTrashManager : MonoBehaviour
{
    [Header("Itens deste PC  (entregues por hora do jogo)")]
    // cada PC tem o seu proprio conjunto de TrashItems - isto permite conteudo diferente por maquina sem logica centralizada
    [SerializeField] private List<TrashItem> itensDestePc = new List<TrashItem>();

    // itens ja entregues ao TrashBinUI deste PC
    private List<TrashItem> itens = new List<TrashItem>();
    // itens ainda a aguardar a sua hora de spawn - percorridos a cada frame em Update
    private List<TrashItem> pendentes = new List<TrashItem>();

    // o TrashBinUI subscreve este evento para adicionar as linhas de UI quando um item chega
    public event System.Action<TrashItem> OnItemRecebido;

    void Start()
    {
        foreach (var item in itensDestePc)
        {
            if (item == null) continue;
            // resetamos o estado runtime do asset para garantir que saves anteriores nao contaminam uma sessao nova
            item.ResetarEstadoRuntime();
            pendentes.Add(item);
        }
    }

    void Update()
    {
        if (pendentes.Count == 0) return;

        float horaAtual = TimeManager.Instance.GetCurrentTimeInHours();

        // percorremos de tras para a frente para remover enquanto iteramos sem partir os indices
        for (int i = pendentes.Count - 1; i >= 0; i--)
        {
            var item = pendentes[i];
            if (horaAtual >= item.spawnHour)
            {
                pendentes.RemoveAt(i);
                EntregarItem(item);
            }
        }
    }

    private void EntregarItem(TrashItem item)
    {
        if (item.entregue) return;
        item.entregue = true;
        itens.Add(item);
        // notifica o TrashBinUI para criar a linha de UI correspondente
        OnItemRecebido?.Invoke(item);
    }

    // injecao manual usada por triggers narrativos para forcar a entrega de um item fora da hora de spawn
    public void ReceberItem(TrashItem item)
    {
        if (itens.Contains(item)) return;
        EntregarItem(item);
    }

    public void ApagarItem(TrashItem item)
    {
        itens.Remove(item);
    }

    // devolve uma copia para o TrashBinUI nao modificar a lista interna acidentalmente
    public List<TrashItem> GetItens() => new List<TrashItem>(itens);
}