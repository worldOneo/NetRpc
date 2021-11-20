using NetRpc.Common;
using System.IO;

namespace NetRpc.Demo
{
  public class KeyExchange : IdMessage
  {
    public byte[] Key;
    public override int Type()
    {
      return (int)MessageType.KEY_EXCHANGE;
    }

    public override byte[] Encode()
    {
      var mem = new MemoryStream();
      var writer = new BinaryWriter(mem);
      writer.Write(Key.Length);
      writer.Write(Key);
      writer.Flush();
      return Encode(mem.ToArray());
    }
    public override void Decode(byte[] data)
    {
      var reader = new BinaryReader(new MemoryStream(data));
      Decode(reader);
      var len = reader.ReadInt32();
      var key = reader.ReadBytes(len);
      Key = key;
    }
  }
}