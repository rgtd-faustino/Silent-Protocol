using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelInventory : MonoBehaviour
{
    public static IntelInventory Instance;

    [Header("Dossier UI")]
    public GameObject dossierPanel;
    public Transform entryListContent;
    public GameObject entryButtonPrefab;

    [Header("Painel de Detalhe")]
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtCategoria;
    public TextMeshProUGUI txtLocalizacao;
    public TextMeshProUGUI txtConteudo;

    private List<IntelItem> entradas = new List<IntelItem>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleDossier();
    }

    public void ToggleDossier()
    {
        bool abrir = !dossierPanel.activeSelf;
        dossierPanel.SetActive(abrir);

        if (abrir)
        {
            UIManager.Instance.ChangeCursorState(CursorLockMode.None);
            PlayerController.Instance.canMoveRotate = false;
        }
        else
        {
            UIManager.Instance.ChangeCursorState(CursorLockMode.Locked);
            PlayerController.Instance.canMoveRotate = true;
        }
    }

    public void AdicionarIntel(IntelItem item)
    {
        if (entradas.Contains(item)) return;

        entradas.Add(item);

        // cria o botão na lista
        var btn = Instantiate(entryButtonPrefab, entryListContent);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();
        label.text = item.titulo;
        btn.GetComponent<Button>().onClick.AddListener(() => MostrarDetalhe(item));
    }

    void MostrarDetalhe(IntelItem item)
    {
        txtTitulo.text = item.titulo;
        txtCategoria.text = item.categoria;
        txtLocalizacao.text = item.localizacao;
        txtConteudo.text = item.conteudo;
    }
}