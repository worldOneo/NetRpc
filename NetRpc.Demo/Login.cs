using System.IO;
using NetRpc.Messages;

namespace NetRpc.Demo
{
  public class Login : IdMessage
  {
    public string username { get; set; }
    public string password { get; set; }

    public override int Type()
    {
      return (int)MessageType.LOGIN;
    }

    public override byte[] Encode()
    {
      var stream = new MemoryStream();
      var writer = new BinaryWriter(stream);
      writer.Write(username);
      writer.Write(password);
      return Encode(stream.ToArray());
    }

    public override void Decode(byte[] data)
    {
      var stream = new BinaryReader(new MemoryStream(data));
      Decode(stream);
      username = stream.ReadString();
      password = stream.ReadString();
    }
  }
}
