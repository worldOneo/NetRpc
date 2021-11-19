using NetRpc.Common;
using System.IO;

namespace NetRpc.Demo
{
  public class Alive : IdMessage
  {
    public override int Type()
    {
      return (int)MessageType.ALIVE;
    }

    public override byte[] Encode() => Encode(new byte[0]);
    public override void Decode(byte[] data) => Decode(new BinaryReader(new MemoryStream(data)));
  }
}