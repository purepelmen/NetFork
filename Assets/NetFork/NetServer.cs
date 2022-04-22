﻿using System;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System.IO;

public class NetServer : MonoBehaviour
{
    public NetConnection FirstConnection => _connections.Count > 0 ? _connections[0] : null;
    public MessageHandler MessageHandler { get; private set; }

    [Header("Initial Server Settings")]
    public int MaxClients = 10;
    public ushort Port = 7777;

    [SerializeField] private EnetTransport _transport;

    private Dictionary<uint, NetConnection> _connections;
    private NetBuffers _buffers;

    private void Start()
    {
        if(DebugErrorIfNoTransport()) return;

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
        if(_transport == null) return;
        if(_transport.IsStarted)
            StopServer();

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

    public void StartServer()
    {
        if(DebugErrorIfNoTransport()) return;
        MessageHandler = new MessageHandler();

        _connections.Clear();
        _transport.StartServer(Port, MaxClients);
    }

    public void StopServer()
    {
        if(DebugErrorIfNoTransport()) return;
        MessageHandler = null;
        
        _transport.StopTransport();
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

    private bool DebugErrorIfNoTransport()
    {
        if(_transport != null) return false;

        Debug.LogError("No EnetTransport attached to NetServer");
        enabled = false;

        return true;
    }
}