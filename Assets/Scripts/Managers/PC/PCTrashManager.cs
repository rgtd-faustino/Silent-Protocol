using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gere a lógica do TrashBin de UM PC específico.
/// Coloca este script no mesmo GameObject que o TrashBinUI.
/// Arrasta os TrashItem assets para a lista 'itensDestePc' no Inspector.
/// </summary>
public class PCTrashManager : MonoBehaviour
{
    [Header("Itens deste PC  (entregues por hora do jogo)")]
    [SerializeField] private List<TrashItem> itensDestePc = new List<TrashItem>();

    private List<TrashItem> itens = new List<TrashItem>();
    private List<TrashItem> pendentes = new List<TrashItem>();

    public event System.Action<TrashItem> OnItemRecebido;

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Start()
    {
        foreach (var item in itensDestePc)
        {
            if (item == null) continue;
            item.ResetarEstadoRuntime();
            pendentes.Add(item);
        }
    }

    void Update()
    {
        if (pendentes.Count == 0) return;

        float horaAtual = TimeManager.Instance.GetCurrentTimeInHours();

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

    // ------------------------------------------------------------------ //
    // Lógica interna                                                        //
    // ------------------------------------------------------------------ //

    private void EntregarItem(TrashItem item)
    {
        if (item.entregue) return;
        item.entregue = true;
        itens.Add(item);
        OnItemRecebido?.Invoke(item);
    }

    // ------------------------------------------------------------------ //
    // API pública                                                           //
    // ------------------------------------------------------------------ //

    /// <summary>Injeta um item em runtime (triggers, eventos de missão, etc.)</summary>
    public void ReceberItem(TrashItem item)
    {
        if (itens.Contains(item)) return;
        EntregarItem(item);
    }

    /// <summary>Remove definitivamente um item do TrashBin.</summary>
    public void ApagarItem(TrashItem item)
    {
        itens.Remove(item);
    }

    public List<TrashItem> GetItens() => new List<TrashItem>(itens);
}