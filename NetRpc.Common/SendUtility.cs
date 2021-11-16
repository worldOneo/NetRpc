using System.IO;
using System;
using System.Threading.Tasks;

namespace NetRpc.Common
{
  public class SendUtility
  {


    internal class MessageWriter
    {
      private Stream stream;
      public MessageWriter(Stream stream)
      {
        this.stream = stream;
      }

      public async Task write(Message message)
      {
        var data = message.Encode();
        var size = data.Length;
        var type = message.Type();
        await AsyncWriteBuff(stream, BitConverter.GetBytes(size));
        await AsyncWriteBuff(stream, BitConverter.GetBytes(type));
        await AsyncWriteBuff(stream, data);
      }
    }

    internal class MessageReader
    {
      public const int SIZE_FIELD_SIZE = 4;
      public const int TYPE_FIELD_SIZE = 4;
      private byte[] msg;
      private byte[] size = new byte[SIZE_FIELD_SIZE];
      private byte[] type = new byte[TYPE_FIELD_SIZE];
      private Stream stream;
      private uint max = 0;
      public MessageReader(Stream stream, uint max)
      {
        this.stream = stream;
        this.max = max;
      }

      public async Task<RawMessage> read()
      {
        await AsyncReadBuff(stream, this.size);
        uint msgSize = BitConverter.ToUInt32(size);
        if (msgSize > max)
        {
          return null;
        }
        msg = new byte[msgSize];
        await AsyncReadBuff(stream, this.type);
        await AsyncReadBuff(stream, this.msg);
        return new RawMessage()
        {
          buff = msg,
          type = BitConverter.ToInt32(type)
        };
      }
    }

    public static async Task AsyncWriteBuff(Stream stream, byte[] buff)
    {
      await stream.WriteAsync(buff, 0, buff.Length);
    }

    public static async Task<int> AsyncReadBuff(Stream stream, byte[] buff)
    {
      int read = 0;
      do
      {
        read += await stream.ReadAsync(buff, read, buff.Length);
      } while (read < buff.Length);
      return read;
    }

    public static Task SendMessage(Stream stream, Message message)
    {
      return new MessageWriter(stream).write(message);
    }

    public static Task<RawMessage> ReadFrame(Stream stream, uint max)
    {
      return new MessageReader(stream, max).read();
    }
  }

  public class RawMessage
  {
    public int type;
    public byte[] buff;
  }
}