using UnityEngine;

[CreateAssetMenu(menuName = "TrashBin/Novo Item")]
public class TrashItem : ScriptableObject
{
    [Header("Contedo")]
    public string titulo;

    [TextArea(5, 20)]
    public string corpo;

    [Header("Intel")]
    public bool temIntel = false;
    [Tooltip("Se temIntel = true, este IntelItem ser guardado no inventrio ao clicar 'Guardar Intel'")]
    public IntelItem intelAssociado;

    [Header("Entrega")]
    [Tooltip("Hora do jogo em que este item aparece no TrashBin.\n" +
             "Ex: 9.0 = 09:00  |  14.5 = 14:30")]
    [Range(0f, 23.99f)]
    public float spawnHour = 9f;

    // ------------------------------------------------------------------ //
    // Estado runtime                                                        //
    // ------------------------------------------------------------------ //
    [HideInInspector] public bool entregue = false;

    public void ResetarEstadoRuntime()
    {
        entregue = false;
    }
}