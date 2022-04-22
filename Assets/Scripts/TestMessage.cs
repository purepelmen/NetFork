using System.IO;

public struct TestMessage : INetMessage
{
    public int BirthdayYear;

    public int Id => 11;

    public void Deserialize(BinaryReader reader)
    {
        BirthdayYear = reader.ReadInt32();
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(BirthdayYear);
    }
}
