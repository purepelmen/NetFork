using System;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System.IO;
using System.Linq;

public class NetServer : MonoBehaviour
{
    public MessageHandler MessageHandler { get; private set; }
    public bool IsStarted => _transport.IsStarted;

    public event Action<NetConnection> Connected;
    public event Action<NetConnection> Disconnected;
    public event Action Started;
    public event Action Stopped;

    [SerializeField] private EnetTransport _transport;

    [Header("Initial Server Settings")]
    public int MaxClients = 10;
    public ushort Port = 7777;

    private Dictionary<uint, NetConnection> _connections;
    private NetBuffers _buffers;

    private void Start()
    {
        _transport.Initialize();

        _connections = new Dictionary<uint, NetConnection>();
        _buffers = new NetBuffers(1024);

        _transport.Started += OnStarted;
        _transport.Stopped += OnStopped;

        _transport.Connected += OnClientConnected;
        _transport.Disconnected += OnClientDisconnected;
        _transport.Timeout += OnClientTimeout;

        _transport.DataReceived += OnDataReceived;
    }

    private void OnDestroy()
    {
        _transport.Initialize();

        _transport.Started -= OnStarted;
        _transport.Stopped -= OnStopped;

        _transport.Connected -= OnClientConnected;
        _transport.Disconnected -= OnClientDisconnected;
        _transport.Timeout -= OnClientTimeout;
    }

    private void Update()
    {
        _transport.Update();
    }

    /// <summary>Starts the server and listen for incoming connections.</summary>
    public void StartServer()
    {
        if(_transport.IsStarted)
        {
            Debug.LogWarning("Server -> Already started, first stop it");
            return;
        }

        if(MaxClients > Library.maxPeers)
        {
            Debug.LogError($"Server -> Too high max clients number, the maximum: {Library.maxPeers}");
            return;
        }

        MessageHandler = new MessageHandler();
        _connections.Clear();

        _transport.StartServer(Port, MaxClients);
    }

    /// <summary>Disconnects all clients and stops the server.</summary>
    public void StopServer()
    {
        if(_transport.IsStarted == false)
        {
            Debug.LogWarning("Server -> Wasn't started");
            return;
        }

        _connections.Values.All((conn) =>
        {
            Disconnect(conn, (uint) StoppedReason.ServerClosing);
            return false;
        });

        MessageHandler = null;
        _transport.StopTransport();
    }

    public NetConnection GetConnection(uint id)
    {
        if(_connections.ContainsKey(id) == false)
            return null;

        return _connections[id];
    }

    public void Disconnect(NetConnection connection)
    {
        Disconnect(connection, (uint) StoppedReason.RemotelyDisconnected);
    }

    private void Disconnect(NetConnection connection, uint disconnectionCode)
    {
        if(connection == null)
        {
            Debug.LogWarning("Server -> Disconnect -> Connection is null");
            return;
        }

        if(_connections.ContainsKey(connection.Id) == false)
        {
            Debug.LogWarning("Server -> Disconnect -> Connection wasn't found");
            return;
        }

        connection.Disconnect(disconnectionCode);
        RemoveConnection(connection.Id);
    }

    private void RemoveConnection(uint id)
    {
        NetConnection connection = GetConnection(id);
        _connections.Remove(id);

        Disconnected?.Invoke(connection);
    }

    private void OnStarted()
    {
        Debug.Log($"Server -> Started, listening on port: {Port}");
        Started?.Invoke();
    }

    private void OnStopped()
    {
        Debug.Log("Server -> Stopped, resources are released");
        Stopped?.Invoke();
    }

    private void OnClientConnected(Peer peer)
    {
        Debug.Log("Server -> Client connected: ID: " + peer.ID + ", IP: " + peer.IP);

        NetConnection newConnection = new NetConnection(_buffers, peer);
        _connections.Add(newConnection.Id, newConnection);

        Connected?.Invoke(newConnection);
    }

    private void OnClientDisconnected(Peer peer, uint data)
    {
        Debug.Log("Server -> Client disconnected: ID: " + peer.ID + ", IP: " + peer.IP);
        uint id = peer.ID;

        RemoveConnection(id);
    }

    private void OnClientTimeout(Peer peer)
    {
        Debug.Log("Server -> Client timeout: ID: " + peer.ID + ", IP: " + peer.IP);
        uint id = peer.ID;

        RemoveConnection(id);
    }

    private void OnDataReceived(Peer peer, Packet packet)
    {
        NetConnection connection = _connections[peer.ID];
        packet.CopyTo(_buffers.ReceiveBuffer);

        _buffers.Reader.BaseStream.Seek(0, SeekOrigin.Begin);
        MessageHandler.Handle(connection, _buffers.Reader);
    }
}
