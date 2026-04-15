// ConversationData.cs
// ScriptableObject — define no Inspector as conversas importantes do jogo
// Menu: Assets > Create > WireShark > Conversation Data

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationData", menuName = "WireShark/Conversation Data")]
public class ConversationData : ScriptableObject
{
    [Tooltip("Lista de conversas predefinidas com intel importante")]
    public List<ConversationEntry> conversations = new List<ConversationEntry>();
}

[Serializable]
public class ConversationEntry
{
    [Header("Identificação")]
    public string conversationId;   // ex: "CONV-192-10"
    public string srcIP;            // ex: "192.168.1.2"
    public string dstIP;            // ex: "10.0.0.3"
    public string protocol;         // "TCP" ou "UDP"
    public string encryptionType;   // "AES" ou "DES"

    [Header("Dia em que aparece")]
    [Tooltip("Em que dia do jogo esta conversa começa a aparecer (1-5)")]
    public int dayToAppear = 1;

    [Header("Mensagens da conversa")]
    [Tooltip("Cada entrada é uma mensagem da conversa por ordem")]
    public List<MessageEntry> messages = new List<MessageEntry>();
}

[Serializable]
public class MessageEntry
{
    [TextArea(2, 4)]
    [Tooltip("Texto plano — será encriptado automaticamente pelo PacketGenerator")]
    public string plainText;

    [Tooltip("Se true, este pacote aparece ao vivo no stream. Se false, já passou e vai para o histórico")]
    public bool appearsLive = true;

    [Tooltip("Se true, este pacote tem intel importante para o jogador")]
    public bool isImportant = false;
}