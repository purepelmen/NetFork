using System.IO;

public struct ServerInfoMessage : INetMessage
{
    public string UnityVersion;
    public string CurrentDate;

    public int Id => 10;

    public void Deserialize(BinaryReader reader)
    {
        UnityVersion = reader.ReadString();
        CurrentDate = reader.ReadString();
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(UnityVersion);
        writer.Write(CurrentDate);
    }
}
