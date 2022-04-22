using System.Collections;
using System.Collections.Generic;
using ENet;
using UnityEngine;

public class TestingMessages : MonoBehaviour
{
    [SerializeField] private NetServer _server;
    [SerializeField] private NetClient _client;

    System.Diagnostics.Stopwatch _stopwatch;

    [ContextMenu("Register")]
    public void Register()
    {
        _server.StartServer();
        _client.Connect();

        _server.MessageHandler.Register<TestMessage>(OnTestMessage);
        _client.MessageHandler.Register<ServerInfoMessage>(OnServerInfoMessage);
    }

    [ContextMenu("Send from client")]
    public void SendClient()
    {
        _client.Send<TestMessage>(new TestMessage() { BirthdayYear = 2020 }, PacketFlags.Reliable);
    }

    [ContextMenu("Send from server")]
    public void SendServer()
    {
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _server.FirstConnection.Send<ServerInfoMessage>(new ServerInfoMessage() 
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
