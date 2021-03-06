using System;
using ENet;
using UnityEngine;

[Serializable]
public class EnetTransport
{
    public bool IsStarted => _host != null;

    public event Action Started;
    public event Action Stopped;

    public event Action<Peer> Connected;
    public event Action<Peer, uint> Disconnected;
    public event Action<Peer> Timeout;
    public event Action<Peer, Packet> DataReceived;

    [Header("Initial Transport Settings")]
    [SerializeField] private uint _timeoutTime = 5000;

    private static bool _isEnetInitialized = false;

    private Peer? _localPeer;
    private Host _host;

    public void Initialize()
    {
        if(_isEnetInitialized) return;

        Library.Initialize();
        _isEnetInitialized = true;
    }

    public void Deinitialize()
    {
        if(_host != null)
            StopTransport();

        if(_isEnetInitialized)
        {
            Library.Deinitialize();
            _isEnetInitialized = false;
        }
    }

    public void Update()
    {
        if(_host == null) return;

        ENet.Event netEvent;
        if(_host.Service(0, out netEvent) <= 0)
            return;
        
        switch(netEvent.Type)
        {
            case ENet.EventType.None:
                break;

            case ENet.EventType.Connect:
                if(_localPeer == null)
                    netEvent.Peer.Timeout(_timeoutTime, _timeoutTime, _timeoutTime);

                Connected?.Invoke(netEvent.Peer);
                break;

            case ENet.EventType.Disconnect:
                Disconnected?.Invoke(netEvent.Peer, netEvent.Data);
                break;

            case ENet.EventType.Timeout:
                Timeout?.Invoke(netEvent.Peer);
                break;

            case ENet.EventType.Receive:
                DataReceived?.Invoke(netEvent.Peer, netEvent.Packet);
                netEvent.Packet.Dispose();
                break;
        }
    }

    public void StartServer(ushort port, int maxPeers)
    {
        if(_host != null) return;

        Address address = new Address();
        _host = new Host();

        address.Port = port;
        _host.Create(address, maxPeers);
        Started?.Invoke();
    }

    public void StartClient(string ipAddress, ushort port)
    {
        if(_host != null) return;

        Address address = new Address();
        address.SetHost(ipAddress);
        address.Port = port;

        _host = new Host();
        _host.Create();

        _localPeer = _host.Connect(address);
        _localPeer.Value.Timeout(_timeoutTime, _timeoutTime, _timeoutTime);
        Started?.Invoke();
    }

    public void StopTransport()
    {
        if(_host == null) return;

        _localPeer?.DisconnectNow(0U);
        Update();

        _localPeer = null;
        _host.Dispose();
        _host = null;

        Stopped?.Invoke();
    }
}
