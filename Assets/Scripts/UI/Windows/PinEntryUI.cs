using System;
using TMPro;
using UnityEngine;

public class PinEntryUI : MonoBehaviour
{
    public static PinEntryUI Instance;

    public GameObject painel;
    public TMP_InputField campoPin;
    public TMP_Text mensagemErro; // opcional, pode ficar a null

    private string pinAlvo;
    private Action callbackCorreto;
    private Action callbackCancelar;

    private void Awake()
    {
        Instance = this;
        painel.SetActive(false);
    }

    public void AbrirPin(string pinAlvo, Action callbackCorreto, Action callbackCancelar)
    {
        this.pinAlvo = pinAlvo;
        this.callbackCorreto = callbackCorreto;
        this.callbackCancelar = callbackCancelar;

        campoPin.text = "";
        if (mensagemErro != null) mensagemErro.gameObject.SetActive(false);

        painel.SetActive(true);
        PlayerController.Instance.canMoveRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        campoPin.Select();
    }

    // liga isto ao OnClick() do botão "Confirmar"
    public void OnConfirmar()
    {
        Debug.Log($"[PinEntryUI] Comparando '{campoPin.text}' com '{pinAlvo}' -> {campoPin.text == pinAlvo}");
        if (campoPin.text == pinAlvo)
        {
            Debug.Log("[PinEntryUI] Correto! A invocar callback...");
            painel.SetActive(false);
            callbackCorreto?.Invoke();
            Debug.Log("[PinEntryUI] Callback invocado.");
        }
        if (campoPin.text == pinAlvo)
        {
            painel.SetActive(false);
            callbackCorreto?.Invoke();
        }
        else
        {
            if (mensagemErro != null)
            {
                mensagemErro.gameObject.SetActive(true);
                mensagemErro.text = "PIN incorreto";
            }
            campoPin.text = "";
            campoPin.Select();
        }
    }

    // liga isto ao OnClick() do botão "Cancelar"
    public void OnCancelar()
    {
        painel.SetActive(false);

        PlayerController.Instance.canMoveRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        callbackCancelar?.Invoke();
    }
}