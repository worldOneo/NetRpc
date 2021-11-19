using NetRpc.Common;
using System.IO;

namespace NetRpc.Demo
{
  public class Login : IdMessage
  {
    public string Username { get; set; }
    public string Password { get; set; }

    public override int Type()
    {
      return (int)MessageType.LOGIN;
    }

    public override byte[] Encode()
    {
      var stream = new MemoryStream();
      var writer = new BinaryWriter(stream);
      writer.Write(Username);
      writer.Write(Password);
      return Encode(stream.ToArray());
    }

    public override void Decode(byte[] data)
    {
      var stream = new BinaryReader(new MemoryStream(data));
      Decode(stream);
      Username = stream.ReadString();
      Password = stream.ReadString();
    }
  }
}
