// NetworkSchedule.cs
// ScriptableObject onde os programadores definem os pacotes por dia
// Botão direito no Project -> Create -> WireShark -> Network Schedule
// Cria um asset por dia (Day1Network, Day2Network, etc.)

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Day1Network", menuName = "WireShark/Network Schedule")]
public class NetworkSchedule : ScriptableObject
{
    [Tooltip("Lista de pacotes que aparecem neste dia. Adiciona quantos quiseres com o '+'")]
    public List<ScheduledPacket> packets = new List<ScheduledPacket>();
}

[Serializable]
public class ScheduledPacket
{
    [Header("Identificação")]
    [Tooltip("ID único desta conversa. Ex: CONV-CEO-01")]
    public string conversationId;

    [Tooltip("Índice desta mensagem dentro da conversa (1, 2, 3...)")]
    public int messageIndex = 1;

    [Header("Rede")]
    public string srcIP = "192.168.1.2";
    public string dstIP = "10.0.0.3";

    [Tooltip("TCP ou UDP")]
    public string protocol = "TCP";

    [Header("Encriptação")]
    [Tooltip("Se false, o HexData mostra o texto em claro")]
    public bool isEncrypted = true;

    [Tooltip("Só usado se isEncrypted = true")]
    public EncryptionType encryptionType = EncryptionType.AES;

    [Header("Conteúdo")]
    [TextArea(2, 4)]
    [Tooltip("Texto da mensagem. Se isEncrypted = false aparece em claro no HexData")]
    public string plainText;

    [Tooltip("Se true, aparece com o indicador de pacote importante (ponto verde)")]
    public bool isImportant = false;

    [Header("Hora de envio")]
    [Tooltip("Hora do jogo em que este pacote é enviado. Ex: 14.5 = 14:30")]
    [Range(0f, 23.99f)]
    public float spawnHour = 9f;
}

public enum EncryptionType { AES, DES }