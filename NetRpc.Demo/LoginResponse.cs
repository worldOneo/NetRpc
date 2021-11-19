using System.IO;
using NetRpc.Common;

namespace NetRpc.Demo
{
  public class LoginResponse : IdMessage
  {
    public bool Successful { get; set; }

    public override int Type()
    {
      return (int)MessageType.LOGIN_RESPONSE;
    }
    public override byte[] Encode()
    {
      return Encode(new byte[] { Successful ? (byte)0x1 : (byte)0x0 });
    }

    public override void Decode(byte[] data)
    {
      var memstream = new BinaryReader(new MemoryStream(data));
      Decode(memstream);
      this.Successful = memstream.ReadByte() == 1;
    }
  }
}