using System.Collections;
using System.Collections.Generic;
using System.IO;
using ENet;
using UnityEngine;

public class NetClient : MonoBehaviour
{
    public MessageHandler MessageHandler { get; private set; }

    [Header("Initial Client Settings")]
    public string ServerIP = "localhost";
    public ushort Port = 7777;

    [SerializeField] private EnetTransport _transport;

    private NetBuffers _buffers;
    private Peer _localPeer;

    private void Start()
    {
        if(DebugErrorIfNoTransport()) return;
        _buffers = new NetBuffers(1024);

        _transport.Started += OnStarted;
        _transport.Stopped += OnStopped;

        _transport.Connected += OnConnected;
        _transport.Disconnected += OnDisconnected;
        _transport.Timeout += OnTimeout;

        _transport.DataReceived += OnDataReceived;
    }

    private void OnDestroy()
    {
        if(_transport == null) return;
        if(_transport.IsStarted)
            StopClient();

        _transport.Started -= OnStarted;
        _transport.Stopped -= OnStopped;

        _transport.Connected -= OnConnected;
        _transport.Disconnected -= OnDisconnected;
        _transport.Timeout -= OnTimeout;
    }

    private void Update()
    {
        if(DebugErrorIfNoTransport()) return;
        _transport.UpdateTransport();
    }

    /// <summary>Starts a client and tries to connect to the server.</summary>
    public void Connect()
    {
        if(DebugErrorIfNoTransport()) return;

        MessageHandler = new MessageHandler();
        _transport.StartClient(ServerIP, Port);
    }

    /// <summary>Disconnects from the server and stops the client.
    /// Must be used to stop connection attempt or disconnect from the server.</summary>
    public void StopClient()
    {
        if(DebugErrorIfNoTransport()) return;

        MessageHandler = null;
        _transport.StopTransport();
    }

    /// <summary>Sends a message to the server.</summary>
    public void Send<T>(T message, PacketFlags sendFlags) where T : struct, INetMessage
    {
        _buffers.Writer.Seek(0, SeekOrigin.Begin);
        _buffers.Writer.Write(message.Id);
        message.Serialize(_buffers.Writer);
        
        Packet packet = default;
        packet.Create(_buffers.SendBuffer, (int) _buffers.Writer.BaseStream.Position, sendFlags);
        _localPeer.Send(0, ref packet);
    }

    /// <summary>Called when the client is started and attempts to connect.</summary>
    private void OnStarted()
    {
        Debug.Log("Client -> Started, attepmts to connect to server");
    }

    /// <summary>Called when the client is stopped and released all resources.</summary>
    private void OnStopped()
    {
        Debug.Log("Client - Stopped, resources are released");
        _localPeer = default;
    }

    private void OnConnected(Peer peer)
    {
        Debug.Log("Client -> Connected to server");
        _localPeer = peer;
    }

    /// <summary>Called when client disconnected from server.
    /// Called only if client was disconnected remotely.
    /// If connection is terminated localy, only OnStopped will be called.</summary>
    private void OnDisconnected(Peer peer)
    {
        Debug.Log("Client -> Disconnected, client will be stopped");
        _transport.StopTransport();
    }

    /// <summary>Callback when client timeout. Used to release resources
    /// and invoke some events.</summary>
    private void OnTimeout(Peer peer)
    {
        Debug.Log("Client -> Timeout, client will be stopped");
        _transport.StopTransport();
    }

    /// <summary>Called when data is received from server.</summary>
    private void OnDataReceived(Peer peer, Packet packet)
    {
        packet.CopyTo(_buffers.ReceiveBuffer);
        
        _buffers.Reader.BaseStream.Seek(0, SeekOrigin.Begin);
        MessageHandler.Handle(null, _buffers.Reader);
    }

    private bool DebugErrorIfNoTransport()
    {
        if(_transport != null) return false;

        Debug.LogError("No EnetTransport attached to NetClient");
        enabled = false;

        return true;
    }
}
