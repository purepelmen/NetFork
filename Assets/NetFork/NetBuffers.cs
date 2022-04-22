using System.IO;

public class NetBuffers
{
    public BinaryReader Reader { get; private set; }
    public BinaryWriter Writer { get; private set; }

    public byte[] ReceiveBuffer { get; private set; }
    public byte[] SendBuffer { get; private set; }

    private MemoryStream _writeMemoryStream;
    private MemoryStream _readMemoryStream;

    public NetBuffers(int size)
    {
        SendBuffer = new byte[size];
        ReceiveBuffer = new byte[size];

        _writeMemoryStream = new MemoryStream(SendBuffer);
        _readMemoryStream = new MemoryStream(ReceiveBuffer);

        Writer = new BinaryWriter(_writeMemoryStream);
        Reader = new BinaryReader(_readMemoryStream);
    }
}
