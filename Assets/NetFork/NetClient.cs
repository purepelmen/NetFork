using System;
using System.IO;
using ENet;
using UnityEngine;

public class NetClient : MonoBehaviour
{
    public MessageHandler MessageHandler { get; private set; }
    public bool IsStarted => _transport.IsStarted;

    public event Action Started;
    public event Action Connected;
    public event Action<StoppedReason> Stopped;

    [SerializeField] private EnetTransport _transport;

    [Header("Initial Client Settings")]
    public string ServerIP = "localhost";
    public ushort Port = 7777;

    private NetBuffers _buffers;
    private Peer _localPeer;

    private void Start()
    {
        _transport.Initialize();

        _buffers = new NetBuffers(1024);

        _transport.Started += OnStarted;
        _transport.Stopped += OnStopped;

        _transport.Connected += OnConnected;
        _transport.Disconnected += OnRemotelyDisconnected;
        _transport.Timeout += OnTimeout;

        _transport.DataReceived += OnDataReceived;
    }

    private void OnDestroy()
    {
        _transport.Deinitialize();

        _transport.Started -= OnStarted;
        _transport.Stopped -= OnStopped;

        _transport.Connected -= OnConnected;
        _transport.Disconnected -= OnRemotelyDisconnected;
        _transport.Timeout -= OnTimeout;
    }

    private void Update()
    {
        _transport.Update();
    }

    /// <summary>Starts a client and tries to connect to the server.</summary>
    public void Connect()
    {
        if(_transport.IsStarted)
        {
            Debug.LogWarning("Client -> Already started, first stop it");
            return;
        }

        MessageHandler = new MessageHandler();
        _transport.StartClient(ServerIP, Port);
    }

    /// <summary>Disconnects from the server and stops the client.
    /// Must be used to stop connection attempt or disconnect from the server.</summary>
    public void StopClient()
    {
        if(_transport.IsStarted == false)
        {
            Debug.LogWarning("Client -> Wasn't started");
            return;
        }
        
        MessageHandler = null;
        _transport.StopTransport();
        
        Stopped?.Invoke(StoppedReason.LocalStopped);
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

    private void OnStarted()
    {
        Debug.Log("Client -> Started, attepmts to connect to server");
        Started?.Invoke();
    }

    private void OnStopped()
    {
        Debug.Log("Client - Stopped, resources are released");
        _localPeer = default;
    }

    private void OnConnected(Peer peer)
    {
        Debug.Log("Client -> Connected to server");
        _localPeer = peer;

        Connected?.Invoke();
    }

    private void OnRemotelyDisconnected(Peer peer, uint data)
    {
        Debug.Log("Client -> Disconnected, client will be stopped");
        _transport.StopTransport();

        switch((StoppedReason) data)
        {
            case StoppedReason.ServerClosing:
                Stopped?.Invoke(StoppedReason.ServerClosing);
                break;

            default:
                Stopped?.Invoke(StoppedReason.RemotelyDisconnected);
                break;
        }
    }

    private void OnTimeout(Peer peer)
    {
        Debug.Log("Client -> Timeout, client will be stopped");
        _transport.StopTransport();

        Stopped?.Invoke(StoppedReason.Timeout);
    }

    private void OnDataReceived(Peer peer, Packet packet)
    {
        packet.CopyTo(_buffers.ReceiveBuffer);
        
        _buffers.Reader.BaseStream.Seek(0, SeekOrigin.Begin);
        MessageHandler.Handle(null, _buffers.Reader);
    }
}
