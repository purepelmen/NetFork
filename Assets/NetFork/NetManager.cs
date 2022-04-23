using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    public NetServer Server => _server;
    public NetClient Client => _client;

    [SerializeField] private NetServer _server;
    [SerializeField] private NetClient _client;

    private void OnDestroy()
    {
        if(_client != null && _client.IsStarted)
            _client.StopClient();
        
        if(_server != null && _server.IsStarted)
            _server.StopServer();    
    }
}
