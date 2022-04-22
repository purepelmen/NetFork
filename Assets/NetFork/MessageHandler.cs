using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MessageHandler
{
    private Dictionary<int, ServerHandler> _handlers;

    public MessageHandler()
    {
        _handlers = new Dictionary<int, ServerHandler>();
    }

    public void Register<T>(Action<NetConnection, T> handler) where T : struct, INetMessage
    {
        int messageId = default(T).Id;
        _handlers.Add(messageId, (connection, reader) =>
        {
            T message = default;
            message.Deserialize(reader);

            handler(connection, message);
        });
    }
    
    public void Register<T>(Action<T> handler) where T : struct, INetMessage
    {
        int messageId = default(T).Id;
        _handlers.Add(messageId, (connection, reader) =>
        {
            T message = default;
            message.Deserialize(reader);

            handler(message);
        });
    }

    public void Unregister<T>() where T : struct, INetMessage
    {
        int messageId = default(T).Id;
        _handlers.Remove(messageId);
    }

    public void Handle(NetConnection connection, BinaryReader reader)
    {
        int messageId = reader.ReadInt32();
        if(_handlers.TryGetValue(messageId, out ServerHandler handler) == false)
        {
            Debug.LogWarning($"Message Handler -> No handler for message id: {messageId}, disconnecting");
            connection.Disconnect();
            return;
        }

        handler(connection, reader);
    }

    private delegate void ServerHandler(NetConnection connection, BinaryReader reader);
}
