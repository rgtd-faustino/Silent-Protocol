using System.Collections;
using UnityEngine;

public class TerminalManager : MonoBehaviour
{

    private TerminalUI ui;

    private enum TerminalState
    {
        Empty,
        Pasted,
        Decrypted,
        Done,
        AskSave,
        AskRename,
        Renaming,
        Finished
    }

    private TerminalState state = TerminalState.Empty;

    private string pastedContent = "";
    private string hexOutput = "";
    private string hashValue = "";
    private string decodedText = "";

    void Start()
    {
        ui = GetComponent<TerminalUI>();
    }

    public void HandleInput(string val)
    {
        string cmd = val.ToLower().Trim();

        if (state == TerminalState.AskSave)
        {
            HandleSaveAnswer(val); return;
        }
        if (state == TerminalState.AskRename)
        {
            HandleRenameAnswer(val); return;
        }
        if (state == TerminalState.Renaming)
        {
            HandleRenameInput(val); return;
        }

        ui.AddLine("user@crypter:~$ " + val, TerminalUI.LineType.Input);

        if (cmd == ".clear") { ClearTerminal(); return; }
        if (cmd == ".help") { ShowHelp(); return; }
        if (cmd == ".aes") { TryAES(); return; }
        if (cmd == ".des") { TryDES(); return; }
        if (cmd == ".hexdecode") { TryHexDecode(); return; }

        if (!cmd.StartsWith(".") && val.Length > 6)
        {
            PastePacket(val); return;
        }

        ui.AddBlank();
        ui.AddLine("  comando nao reconhecido: " + val, TerminalUI.LineType.Err);
        ui.AddBlank();
    }

    private void PastePacket(string content)
    {
        pastedContent = content;
        state = TerminalState.Pasted;
        hexOutput = "";
        hashValue = "";
        decodedText = "";

        ui.AddBlank();
        ui.AddLine("  [INPUT RECEBIDO]", TerminalUI.LineType.Info);
        ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);

        string preview = pastedContent.Length > 52
            ? pastedContent.Substring(0, 52) + "..."
            : pastedContent;

        ui.AddLine("  " + preview, TerminalUI.LineType.Hex);
        ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
        ui.AddLine("  " + pastedContent.Length + " bytes. estrutura encriptada detetada.", TerminalUI.LineType.Dim);
        ui.AddBlank();
    }

    private void TryAES()
    {
        if (state == TerminalState.Empty)
        {
            ui.AddBlank();
            ui.AddLine("  nenhum pacote carregado.", TerminalUI.LineType.Err);
            ui.AddBlank();
            return;
        }
        if (state == TerminalState.Done || state == TerminalState.Finished)
        {
            ui.AddBlank();
            ui.AddLine("  pacote ja processado.", TerminalUI.LineType.Dim);
            ui.AddBlank();
            return;
        }

        ui.AddBlank();
        ui.AddLine("  > a tentar .AES...", TerminalUI.LineType.Dim);
        StartCoroutine(ProcessAES());
    }

    private IEnumerator ProcessAES()
    {
        yield return new WaitForSeconds(0.5f);


        bool correct = pastedContent.ToUpper().Contains("AES256");
        string extractedHash = ExtractField(pastedContent, "hash:");

        if (correct)
        {
            ui.AddLine("  [OK] chave AES validada.", TerminalUI.LineType.Info);
            ui.AddLine("  a decifrar...", TerminalUI.LineType.Dim);
            yield return new WaitForSeconds(0.7f);

            hexOutput = "48 65 6c 6c 6f 20 4d 75 6e 64 6f";
            hashValue = "extractedHash";
            state = TerminalState.Decrypted;

            ui.AddBlank();
            ui.AddLine("  [OUTPUT]", TerminalUI.LineType.Info);
            ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
            ui.AddLine("  " + hexOutput, TerminalUI.LineType.Hex);
            ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
            ui.AddLine("  HASH=" + hashValue, TerminalUI.LineType.Hash);
            ui.AddBlank();
        }
        else
        {
            ui.AddLine("  [FALHOU] algoritmo incorreto.", TerminalUI.LineType.Err);
            ui.AddBlank();
        }
    }

    private void TryDES()
    {
        if (state == TerminalState.Empty)
        {
            ui.AddBlank();
            ui.AddLine("  nenhum pacote carregado.", TerminalUI.LineType.Err);
            ui.AddBlank();
            return;
        }
        if (state == TerminalState.Done || state == TerminalState.Finished)
        {
            ui.AddBlank();
            ui.AddLine("  pacote ja processado.", TerminalUI.LineType.Dim);
            ui.AddBlank();
            return;
        }

        ui.AddBlank();
        ui.AddLine("  > a tentar .DES...", TerminalUI.LineType.Dim);
        StartCoroutine(ProcessDES());
    }

    private IEnumerator ProcessDES()
    {
        yield return new WaitForSeconds(0.5f);

        bool correct = pastedContent.ToUpper().Contains("DES_PKT");
        string extractedHash = ExtractField(pastedContent, "hash:");

        if (correct)
        {
            ui.AddLine("  [OK] chave DES validada.", TerminalUI.LineType.Info);
            ui.AddLine("  a decifrar...", TerminalUI.LineType.Dim);
            yield return new WaitForSeconds(0.7f);

            hexOutput = "53 65 67 72 65 64 6f 20 65 78 70 6f 73 74 6f";
            hashValue = extractedHash;
            state = TerminalState.Decrypted;

            ui.AddBlank();
            ui.AddLine("  [OUTPUT]", TerminalUI.LineType.Info);
            ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
            ui.AddLine("  " + hexOutput, TerminalUI.LineType.Hex);
            ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
            ui.AddLine("  HASH=" + hashValue, TerminalUI.LineType.Hash);
            ui.AddBlank();
        }
        else
        {
            ui.AddLine("  [FALHOU] algoritmo incorreto.", TerminalUI.LineType.Err);
            ui.AddBlank();
        }
    }
    private string ExtractField(string text, string key)
    {
        int idx = text.IndexOf(key);
        if (idx < 0) return "???";
        int start = idx + key.Length;
        int end = text.IndexOf('_', start);
        return end < 0 ? text.Substring(start) : text.Substring(start, end - start);
    }

    private void TryHexDecode()
    {
        if (state != TerminalState.Decrypted)
        {
            ui.AddBlank();
            ui.AddLine("  sem output para converter.", TerminalUI.LineType.Err);
            ui.AddBlank();
            return;
        }

        ui.AddBlank();
        ui.AddLine("  > a converter...", TerminalUI.LineType.Dim);
        StartCoroutine(ProcessHexDecode());
    }

    private IEnumerator ProcessHexDecode()
    {
        yield return new WaitForSeconds(0.5f);

        string[] bytes = hexOutput.Split(' ');
        string plain = "";
        foreach (string b in bytes)
        {
            if (string.IsNullOrEmpty(b)) continue;
            plain += (char)System.Convert.ToInt32(b, 16);
        }

        decodedText = plain.Trim();
        state = TerminalState.Done;

        ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
        ui.AddLine("  " + decodedText, TerminalUI.LineType.Plain);
        ui.AddLine("  ----------------------------------------", TerminalUI.LineType.Sep);
        ui.AddBlank();
        ui.AddLine("  HASH=" + hashValue + " registado.", TerminalUI.LineType.Hash);
        ui.AddLine("  pacote decifrado com sucesso.", TerminalUI.LineType.Info);
        ui.AddBlank();

        yield return new WaitForSeconds(0.6f);
        AskSaveIntel();
    }

    private void AskSaveIntel()
    {
        ui.AddLine("  ════════════════════════════════════════", TerminalUI.LineType.Sep);
        ui.AddBlank();
        ui.AddLine("  Quer guardar intel? [S/N]", TerminalUI.LineType.Prompt);
        ui.AddBlank();
        state = TerminalState.AskSave;
        ui.SetPrompt("  >");
    }

    private void HandleSaveAnswer(string val)
    {
        string ans = val.Trim().ToLower();
        ui.AddLine("  > " + val, TerminalUI.LineType.Input);
        ui.AddBlank();

        if (ans == "s")
        {
            ui.AddLine("  intel guardada no dossier.", TerminalUI.LineType.Saved);
            ui.AddLine("  HASH=" + hashValue + " | \"" + decodedText + "\"", TerminalUI.LineType.Hash);
            ui.AddBlank();

            // TODO: quando tiveres o IntelManager chamar aqui
            // IntelManager.Instance.AddIntel(...)

            StartCoroutine(DelayThen(0.4f, AskRename));

        }
        else if (ans == "n")
        {
            ui.AddLine("  intel descartada.", TerminalUI.LineType.Dim);
            ui.AddBlank();
            ui.AddLine("  ════════════════════════════════════════", TerminalUI.LineType.Sep);
            FinishTerminal();

        }
        else
        {
            ui.AddLine("  resposta invalida. escreve S ou N.", TerminalUI.LineType.Err);
            ui.AddBlank();
            ui.AddLine("  Quer guardar intel? [S/N]", TerminalUI.LineType.Prompt);
            ui.AddBlank();
        }
    }

    private void AskRename()
    {
        ui.AddLine("  Quer alterar a mensagem? [S/N]", TerminalUI.LineType.Prompt);
        ui.AddBlank();
        state = TerminalState.AskRename;
    }

    private void HandleRenameAnswer(string val)
    {
        string ans = val.Trim().ToLower();
        ui.AddLine("  > " + val, TerminalUI.LineType.Input);
        ui.AddBlank();

        if (ans == "s")
        {
            ui.AddLine("  escreve o novo titulo para esta intel:", TerminalUI.LineType.Dim);
            ui.AddBlank();
            state = TerminalState.Renaming;
            ui.SetPrompt("  titulo >");

        }
        else if (ans == "n")
        {
            ui.AddLine("  titulo mantido: \"" + decodedText + "\"", TerminalUI.LineType.Dim);
            ui.AddBlank();
            ui.AddLine("  ════════════════════════════════════════", TerminalUI.LineType.Sep);
            FinishTerminal();

        }
        else
        {
            ui.AddLine("  resposta invalida. escreve S ou N.", TerminalUI.LineType.Err);
            ui.AddBlank();
            ui.AddLine("  Quer alterar a mensagem? [S/N]", TerminalUI.LineType.Prompt);
            ui.AddBlank();
            state = TerminalState.AskRename;
        }
    }

    private void HandleRenameInput(string val)
    {
        if (string.IsNullOrEmpty(val))
        {
            ui.AddLine("  titulo nao pode estar vazio.", TerminalUI.LineType.Err);
            ui.AddBlank();
            return;
        }

        ui.AddLine("  titulo > " + val, TerminalUI.LineType.Input);
        ui.AddBlank();
        ui.AddLine("  titulo alterado para: \"" + val + "\"", TerminalUI.LineType.Saved);
        ui.AddBlank();
        ui.AddLine("  ════════════════════════════════════════", TerminalUI.LineType.Sep);

        // TODO: quando tiveres o IntelManager
        // IntelManager.Instance.RenameLastIntel(val);

        FinishTerminal();
    }

    private void FinishTerminal()
    {
        state = TerminalState.Finished;
        ui.SetPrompt("user@crypter:~$");
    }

    private void ShowHelp()
    {
        ui.AddBlank();
        ui.AddLine("  CRYPTER aceita comandos que comecam por \".\"", TerminalUI.LineType.Info);
        ui.AddLine("  cola o conteudo do pacote para comecar.", TerminalUI.LineType.Dim);
        ui.AddLine("  os comandos de decifra tens de os descobrir.", TerminalUI.LineType.Dim);
        ui.AddLine("  .clear   — limpa o terminal", TerminalUI.LineType.Dim);
        ui.AddBlank();
    }

    private void ClearTerminal()
    {
        ui.ClearOutput();
        state = TerminalState.Empty;
        pastedContent = "";
        hexOutput = "";
        hashValue = "";
        decodedText = "";
        ui.SetPrompt("user@crypter:~$");
        ui.AddLine("  terminal limpo.", TerminalUI.LineType.Sys);
        ui.AddBlank();
    }

    private IEnumerator DelayThen(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    // No TerminalUI.cs liga o teu botão "Colar" a este método:
    // pasteButton.onClick.AddListener(() => manager.PasteFromClipboard());

    public void PasteFromClipboard()
    {
        if (!GameClipboard.Instance.HasContent())
        {
            ui.AddBlank();
            ui.AddLine("  clipboard vazio. copia um pacote primeiro.",
                       TerminalUI.LineType.Err);
            ui.AddBlank();
            return;
        }

        string content = GameClipboard.Instance.Paste();
        ui.AddLine("  [COLADO DO CLIPBOARD]", TerminalUI.LineType.Dim);
        HandleInput(content);   // reutiliza a lógica que já tens
    }
}