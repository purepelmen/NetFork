using System;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System.IO;

public class NetServer : MonoBehaviour
{
    public MessageHandler MessageHandler { get; private set; }
    public bool IsStarted => _transport.IsStarted;

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

        _transport.Connected += OnConnected;
        _transport.Disconnected += OnDisconnected;
        _transport.Timeout += OnTimeout;

        _transport.DataReceived += OnDataReceived;
    }

    private void OnDestroy()
    {
        _transport.Initialize();

        _transport.Started -= OnStarted;
        _transport.Stopped -= OnStopped;

        _transport.Connected -= OnConnected;
        _transport.Disconnected -= OnDisconnected;
        _transport.Timeout -= OnTimeout;
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

        MessageHandler = null;
        _transport.StopTransport();
    }

    public NetConnection GetConnection(uint id)
    {
        if(_connections.ContainsKey(id) == false)
            return null;

        return _connections[id];
    }

    private void OnStarted()
    {
        Debug.Log($"Server -> Started, listening on port: {Port}");
    }

    private void OnStopped()
    {
        Debug.Log("Server -> Stopped, resources are released");
    }

    private void OnConnected(Peer peer)
    {
        Debug.Log("Server -> Client connected: ID: " + peer.ID + ", IP: " + peer.IP);

        NetConnection newConnection = new NetConnection(_buffers, peer);
        _connections.Add(newConnection.Id, newConnection);
    }

    private void OnDisconnected(Peer peer)
    {
        Debug.Log("Server -> Client disconnected: ID: " + peer.ID + ", IP: " + peer.IP);
        _connections.Remove(peer.ID);
    }

    private void OnTimeout(Peer peer)
    {
        Debug.Log("Server -> Client timeout: ID: " + peer.ID + ", IP: " + peer.IP);
        _connections.Remove(peer.ID);
    }

    private void OnDataReceived(Peer peer, Packet packet)
    {
        NetConnection connection = _connections[peer.ID];
        packet.CopyTo(_buffers.ReceiveBuffer);

        _buffers.Reader.BaseStream.Seek(0, SeekOrigin.Begin);
        MessageHandler.Handle(connection, _buffers.Reader);
    }
}
