using System.IO;

namespace NetRpc.Common
{
  public class SendUtility
  {
    public static void SendMessage(Stream stream, Message message)
    {
      var writer = new BinaryWriter(stream);
      var data = message.Encode();
      var type = message.Type();
      var length = data.Length;
      writer.Write(length);
      writer.Write(type);
      writer.Write(data);
      writer.Flush();
    }

    public static byte[] ReadFrame(Stream stream, out int type, int max = 1_000_000)
    {
      var reader = new BinaryReader(stream);
      var len = reader.ReadInt32();
      if (len > max)
      {
        type = -1;
        return null;
      }
      var buff = new byte[len];
      type = reader.ReadInt32();
      int read = 0;
      while (read < len)
      {
        var newRead = reader.Read(buff, read, len - read);
        if (newRead == -1)
          continue;
        read += newRead;
      }
      return buff;
    }
  }
}