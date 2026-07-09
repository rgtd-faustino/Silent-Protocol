// TerminalManager.cs — ATUALIZADO
// Substituído o hardcode de hex por CryptoHelper real
// .aes e .des agora desencriptam o payload do GameClipboard corretamente

using System.Collections;
using UnityEngine;

public class TerminalManager : MonoBehaviour
{
    private TerminalUI ui;

    private enum TerminalState
    {
        Empty, Pasted, Decrypted, Done, AskSave, AskRename, Renaming, Finished
    }

    private TerminalState state = TerminalState.Empty;

    private string pastedContent = "";
    private string hexOutput = "";
    private string hashValue = "";
    private string decodedText = "";
    private string encryptionType = ""; // "AES" ou "DES" — detetado automaticamente

    void Start()
    {
        ui = GetComponent<TerminalUI>();
    }

    public void HandleInput(string val)
    {
        string cmd = val.ToLower().Trim();

        if (state == TerminalState.AskSave) { HandleSaveAnswer(val); return; }
        if (state == TerminalState.AskRename) { HandleRenameAnswer(val); return; }
        if (state == TerminalState.Renaming) { HandleRenameInput(val); return; }

        ui.AddLine("user@crypter:~$ " + val, TerminalUI.LineType.Input);

        // som de teclado 2D ao submeter um comando no terminal
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.typingKeyboard);

        if (cmd == ".clear") { ClearTerminal(); return; }
        if (cmd == ".help") { ShowHelp(); return; }
        if (cmd == ".aes") { TryDecrypt("AES"); return; }
        if (cmd == ".des") { TryDecrypt("DES"); return; }
        if (cmd == ".hexdecode") { TryHexDecode(); return; }

        // qualquer coisa com mais de 6 chars que não seja comando = pacote colado manualmente
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
        encryptionType = "";
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

        // dica visual se vier do clipboard
        if (GameClipboard.HasContent && GameClipboard.PacketContent == content)
        {
            ui.AddLine("  pacote: " + GameClipboard.PacketId, TerminalUI.LineType.Dim);
            encryptionType = GameClipboard.EncryptionType;
        }

        ui.AddBlank();
    }

    private void TryDecrypt(string attemptedType)
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
        ui.AddLine("  > a tentar ." + attemptedType + "...", TerminalUI.LineType.Dim);
        StartCoroutine(ProcessDecrypt(attemptedType));
    }

    private IEnumerator ProcessDecrypt(string attemptedType)
    {
        yield return new WaitForSeconds(0.5f);

        // se soubermos o tipo (veio do clipboard) validamos, senão o jogador tem de adivinhar
        bool correct;
        if (!string.IsNullOrEmpty(encryptionType))
            correct = (attemptedType == encryptionType);
        else
            correct = TryDecryptPayload(attemptedType, out _); // tenta mesmo desencriptar

        if (correct)
        {
            string decrypted = CryptoHelper.Decrypt(pastedContent, attemptedType);

            if (string.IsNullOrEmpty(decrypted))
            {
                ui.AddLine("  [FALHOU] nao foi possivel decifrar.", TerminalUI.LineType.Err);
                ui.AddBlank();
                yield break;
            }

            ui.AddLine("  [OK] chave " + attemptedType + " validada.", TerminalUI.LineType.Info);
            ui.AddLine("  a decifrar...", TerminalUI.LineType.Dim);
            yield return new WaitForSeconds(0.7f);

            // o output do terminal é o hex do texto decifrado (como antes)
            hexOutput = CryptoHelper.BytesToHexString(System.Text.Encoding.UTF8.GetBytes(decrypted));
            hashValue = CryptoHelper.GenerateHash(decrypted);
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

    // tenta desencriptar e verifica se o resultado é texto legível
    private bool TryDecryptPayload(string encType, out string result)
    {
        try
        {
            result = CryptoHelper.Decrypt(pastedContent, encType);
            return !string.IsNullOrEmpty(result);
        }
        catch
        {
            result = "";
            return false;
        }
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

        // converte o hex de volta para texto plano
        byte[] bytes = CryptoHelper.HexStringToBytes(hexOutput);
        decodedText = System.Text.Encoding.UTF8.GetString(bytes).Trim();
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

            // TODO: IntelManager.Instance.AddIntel(hashValue, decodedText);
            GameClipboard.Clear();

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

        // TODO: IntelManager.Instance.RenameLastIntel(val);

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
        encryptionType = "";
        ui.SetPrompt("user@crypter:~$");
        ui.AddLine("  terminal limpo.", TerminalUI.LineType.Sys);
        ui.AddBlank();
    }

    private IEnumerator DelayThen(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}