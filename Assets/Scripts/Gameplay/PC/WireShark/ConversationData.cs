using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationData", menuName = "WireShark/Conversation Data")]
public class ConversationData : ScriptableObject
{
    // conjunto de conversas no tráfego de pacotes que contêm informação relevante para a narrativa
    public List<ConversationEntry> conversations = new List<ConversationEntry>();
}

[Serializable]
public class ConversationEntry
{
    [Header("Identificação")]
    public string conversationId;
    public string srcIP;
    public string dstIP;
    public string protocol;
    public string encryptionType;

    [Header("Dia em que aparece")]
    public int dayToAppear = 1; // o dia da semana no jogo em que esta conversa começa a ser injetada no tráfego de pacotes

    [Header("Mensagens da conversa")]
    // e estas são as linhas de diálogos que compõem a conversa
    public List<MessageEntry> messages = new List<MessageEntry>();
}

[Serializable]
public class MessageEntry
{
    [TextArea(2, 4)]
    // texto legível que poderá ser ofuscado pelo PacketGenerator
    public string plainText;

    // se for verdadeiro, o pacote é visível na stream em tempo real
    // senão vai apenas para o histórico
    public bool appearsLive = true;

    // indica se este pacote tem dados cruciais que o jogador deve investigar e guardar
    public bool isImportant = false;
}