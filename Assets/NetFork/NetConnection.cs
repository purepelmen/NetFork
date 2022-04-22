using System.IO;
using ENet;

public class NetConnection
{
    public readonly string RemoteIp;
    public readonly ushort RemotePort;
    public readonly uint Id; 

    private NetBuffers _buffers;
    private Peer _peer;

    public NetConnection(NetBuffers buffers, Peer peer)
    {
        _buffers = buffers;
        _peer = peer;

        RemoteIp = peer.IP;
        RemotePort = peer.Port;
        Id = peer.ID;
    }

    public void Send<T>(T message, PacketFlags sendFlags) where T : struct, INetMessage
    {
        _buffers.Writer.Seek(0, SeekOrigin.Begin);
        _buffers.Writer.Write(message.Id);
        message.Serialize(_buffers.Writer);
        
        Packet packet = default;
        packet.Create(_buffers.SendBuffer, (int) _buffers.Writer.BaseStream.Position, sendFlags);
        _peer.Send(0, ref packet);
    }

    public void Disconnect()
    {
        _peer.Disconnect(0U);
    }
}
