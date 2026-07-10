using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationData", menuName = "WireShark/Conversation Data")]
public class ConversationData : ScriptableObject
{
    [Tooltip("Conjunto de conversas que contêm informação relevante para a narrativa.")]
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
    [Tooltip("O dia da semana no jogo em que esta conversa começa a ser injetada no tráfego.")]
    public int dayToAppear = 1;

    [Header("Mensagens da conversa")]
    [Tooltip("Lista sequencial dos pacotes que compõem a troca de mensagens.")]
    public List<MessageEntry> messages = new List<MessageEntry>();
}

[Serializable]
public class MessageEntry
{
    [TextArea(2, 4)]
    [Tooltip("Texto legível que será ofuscado pelo PacketGenerator em runtime.")]
    public string plainText;

    [Tooltip("Se for verdadeiro, o pacote é visível na stream em tempo real. Caso contrário, vai apenas para o histórico.")]
    public bool appearsLive = true;

    [Tooltip("Indica se este pacote tem dados cruciais que o jogador deve investigar e guardar.")]
    public bool isImportant = false;
}