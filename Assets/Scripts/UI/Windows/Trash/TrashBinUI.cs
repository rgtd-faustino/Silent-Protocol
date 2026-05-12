using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Controla a UI do TrashBin de UM PC específico.

public class TrashBinUI : MonoBehaviour
{
    // flag estática para o PlayerController saber se algum TrashBin está aberto
    public static bool AlgumTrashAberto = false;

    // ------------------------------------------------------------------ //
    // Manager local                                                         //
    // ------------------------------------------------------------------ //

    [Header("Manager deste PC")]
    [Tooltip("Auto-detectado se estiver no mesmo GameObject.")]
    public PCTrashManager trashManager;

    // ------------------------------------------------------------------ //
    // Referências de UI                                                     //
    // ------------------------------------------------------------------ //

    [Header("Painel Raiz")]
    public GameObject trashAppPanel;

    [Header("TopBar")]
    public Button btnFechar;

    [Header("Lista")]
    public Transform listContent;           // Content do ScrollRect da lista
    public GameObject trashEntryPrefab;     // prefab de cada linha (Button + TxtTitulo)

    [Header("Detalhe")]
    public GameObject trashDetailPanel;
    public GameObject badgeIntel;           // BadgeIntel (ativo só se temIntel)
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtCorpo;
    public Button btnGuardarIntel;

    // ------------------------------------------------------------------ //
    // Estado interno                                                        //
    // ------------------------------------------------------------------ //

    private TrashItem itemSelecionado = null;
    private List<GameObject> entradasAtivas = new List<GameObject>();
    private HashSet<TrashItem> intelJaGuardada = new HashSet<TrashItem>();

    // ------------------------------------------------------------------ //
    // Unity                                                                 //
    // ------------------------------------------------------------------ //

    void Awake()
    {
        if (trashManager == null)
            trashManager = GetComponent<PCTrashManager>();

        if (trashManager == null)
            Debug.LogError($"[TrashBinUI] Nenhum PCTrashManager encontrado em '{gameObject.name}'.", this);
    }

    void Start()
    {
        if (trashManager == null) return;

        btnFechar.onClick.AddListener(FecharApp);
        btnGuardarIntel.onClick.AddListener(GuardarIntelAtual);

        trashManager.OnItemRecebido += OnItemRecebido;

        trashDetailPanel.SetActive(false);
        AtualizarLista();
    }

    void OnDestroy()
    {
        if (trashManager != null)
            trashManager.OnItemRecebido -= OnItemRecebido;
    }

    // ------------------------------------------------------------------ //
    // API pública                                                           //
    // ------------------------------------------------------------------ //

    public void ToggleApp()
    {
        bool abrir = !trashAppPanel.activeSelf;
        trashAppPanel.SetActive(abrir);
        AlgumTrashAberto = abrir;

        if (abrir)
        {
            UIManager.Instance.ChangeCursorState(CursorLockMode.None);
            PlayerController.Instance.canMoveRotate = false;
            AtualizarLista();
        }
        else
        {
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
        }
    }

    public void FecharApp()
    {
        trashAppPanel.SetActive(false);
        AlgumTrashAberto = false;
        UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
        PlayerController.Instance.canMoveRotate = true;
    }

    // ------------------------------------------------------------------ //
    // Lista                                                                 //
    // ------------------------------------------------------------------ //

    private void AtualizarLista()
    {
        foreach (var go in entradasAtivas) Destroy(go);
        entradasAtivas.Clear();

        foreach (var item in trashManager.GetItens())
            CriarEntrada(item);
    }

    private void CriarEntrada(TrashItem item)
    {
        var go = Instantiate(trashEntryPrefab, listContent);
        entradasAtivas.Add(go);

        // assume que o prefab tem pelo menos um TMP com o título
        var label = go.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = item.titulo;

        go.GetComponent<Button>().onClick.AddListener(() => AbrirItem(item));
    }

    // ------------------------------------------------------------------ //
    // Detalhe                                                               //
    // ------------------------------------------------------------------ //

    private void AbrirItem(TrashItem item)
    {
        itemSelecionado = item;

        txtTitulo.text = item.titulo;
        txtCorpo.text = item.corpo;

        // badge INTEL só aparece se o item tiver intel associada
        if (badgeIntel != null)
            badgeIntel.SetActive(item.temIntel && item.intelAssociado != null);

        bool temIntelDisponivel = item.temIntel && item.intelAssociado != null;
        btnGuardarIntel.gameObject.SetActive(temIntelDisponivel);
        btnGuardarIntel.interactable = !intelJaGuardada.Contains(item);

        var label = btnGuardarIntel.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = intelJaGuardada.Contains(item) ? "Intel Guardada ✓" : "▼ GUARDAR INTEL";

        trashDetailPanel.SetActive(true);
    }

    // ------------------------------------------------------------------ //
    // Ações                                                                 //
    // ------------------------------------------------------------------ //

    private void GuardarIntelAtual()
    {
        if (itemSelecionado == null || !itemSelecionado.temIntel) return;
        if (intelJaGuardada.Contains(itemSelecionado)) return;

        IntelInventory.Instance.AdicionarIntel(itemSelecionado.intelAssociado);
        intelJaGuardada.Add(itemSelecionado);

        btnGuardarIntel.interactable = false;
        var label = btnGuardarIntel.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = "Intel Guardada ✓";
    }

    // ------------------------------------------------------------------ //
    // Eventos                                                               //
    // ------------------------------------------------------------------ //

    private void OnItemRecebido(TrashItem item)
    {
        if (trashAppPanel.activeSelf)
            AtualizarLista();
    }
}