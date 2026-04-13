using UnityEngine;

public class IntelPickup : InteractableObject
{
    public IntelItem item;
    private bool guardado = false;

    void Start()
    {
        objectName = item != null ? item.titulo : "Intel";
    }

    public override void Interact()
    {
        if (guardado) return;

        // E para ler — mostra o conteúdo (podes ligar ao DossierUI mais tarde)
        Debug.Log($"[IntelPickup] A ler: {item.titulo}");
    }

    void Update()
    {
        if (guardado) return;

        // G para guardar no inventário
        if (Input.GetKeyDown(KeyCode.G))
        {
            // só guarda se estiver a apontar para este objeto
            IntelInventory.Instance.AdicionarIntel(item);
            guardado = true;
            gameObject.SetActive(false);
            Debug.Log($"[IntelPickup] Guardado: {item.titulo}");
        }
    }
}