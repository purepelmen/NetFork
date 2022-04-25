using System.Collections;
using System.Collections.Generic;
using ENet;
using UnityEngine;

public class TestingMessages : MonoBehaviour
{
    [SerializeField] private NetManager _manager;

    System.Diagnostics.Stopwatch _stopwatch;

    [ContextMenu("Start and register")]
    public void StartAndRegister()
    {
        _manager.Server.StartServer();
        _manager.Client.Connect();

        _manager.Server.MessageHandler.Register<TestMessage>(OnTestMessage);
        _manager.Client.MessageHandler.Register<ServerInfoMessage>(OnServerInfoMessage);

        _manager.Client.Stopped += (reason) =>
        {
            Debug.Log($"Client -> Disconnected with reason: {reason}");
        };
    }

    [ContextMenu("Send from client")]
    public void SendClient()
    {
        NetConnection localConnection = _manager.Client.LocalConnection;
        localConnection.Send(new TestMessage() { BirthdayYear = 2020 });
    }

    [ContextMenu("Send from server")]
    public void SendServer()
    {
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _manager.Server.GetConnection(0).Send(new ServerInfoMessage() 
        { 
            UnityVersion = Application.unityVersion,
            CurrentDate = System.DateTime.Now.ToString()
        }, PacketFlags.None);

        Debug.Log("Time elapsed from send: " + _stopwatch.ElapsedMilliseconds);
    }

    private void OnTestMessage(NetConnection connection, TestMessage message)
    {
        Debug.Log($"Server received birthday: {message.BirthdayYear}");
    }

    private void OnServerInfoMessage(NetConnection connection, ServerInfoMessage message)
    {
        _stopwatch.Stop();
        Debug.Log("Time elapsed: " + _stopwatch.ElapsedMilliseconds);
        Debug.Log($"Client received server info: Unity version: {message.UnityVersion}, current date: {message.CurrentDate}");
    }
}
