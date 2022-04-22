using System.IO;

public interface INetMessage
{
    int Id { get; }

    void Serialize(BinaryWriter writer);
    void Deserialize(BinaryReader reader);
}
