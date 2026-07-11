using System.Collections.Generic;
using UnityEngine;

public class PCTrashManager : MonoBehaviour
{
    [Header("Itens deste PC  (entregues por hora do jogo)")]
    // cada PC tem o seu proprio conjunto de TrashItems de modo a poderem ter conteúdos diferentes
    [SerializeField] private List<TrashItem> itensDestePc = new List<TrashItem>();

    // itens já entregues ao TrashBinUI deste PC
    private List<TrashItem> itens = new List<TrashItem>();

    // itens ainda a aguardar a sua hora de spawn
    private List<TrashItem> pendentes = new List<TrashItem>();

    // devolve uma copia para o TrashBinUI não modificar a lista interna acidentalmente
    public List<TrashItem> GetItens() => new List<TrashItem>(itens);

    // este evento fica aqui (e não no GameEvent.cs) de propósito porque é um evento por instância, não global
    // cada PC tem o seu próprio PCTrashManager e o seu próprio TrashBinUI, e cada TrashBinUI só deve reagir aos itens do seu próprio PC
    // se isto fosse para o GameEvent (estático/global), todos os TrashBinUI de todos os PCs receberiam a notificação de qualquer item entregue em qualquer PC, obrigando a
    // filtrar manualmente por PC em cada subscritor
    public event System.Action<TrashItem> OnItemRecebido;

    void Start()
    {
        foreach (var item in itensDestePc)
        {
            if (item == null) 
                continue;

            // damos reset ao estado runtime do asset para garantir que saves anteriores não contaminam uma sessão nova
            item.ResetarEstadoRuntime();
            pendentes.Add(item);
        }
    }

    void Update()
    {
        if (pendentes.Count == 0)
            return;

        float horaAtual = TimeManager.Instance.GetCurrentTimeInHours();

        // percorremos de trás para a frente para remover enquanto iteramos sem partir os índices
        for (int i = pendentes.Count - 1; i >= 0; i--)
        {
            var item = pendentes[i];
            if (horaAtual >= item.spawnHour)
            {
                // quando já passar da hora, entregamos o item ao lixo para aparecer
                pendentes.RemoveAt(i);
                EntregarItem(item);
            }
        }
    }

    private void EntregarItem(TrashItem item)
    {
        if (item.entregue)
            return;

        item.entregue = true;
        itens.Add(item);
        // notifica o TrashBinUI para criar a linha de UI correspondente
        OnItemRecebido?.Invoke(item);
    }

}