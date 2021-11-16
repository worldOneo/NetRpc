using System.IO;
using System;
using System.Threading.Tasks;

namespace NetRpc.Common
{
  public class SendUtility
  {


    internal class MessageWriter
    {
      private Stream _stream;
      public MessageWriter(Stream stream)
      {
        this._stream = stream;
      }

      public async Task write(IMessage message)
      {
        var data = message.Encode();
        var size = data.Length;
        var type = message.Type();
        await AsyncWriteBuff(_stream, BitConverter.GetBytes(size));
        await AsyncWriteBuff(_stream, BitConverter.GetBytes(type));
        await AsyncWriteBuff(_stream, data);
      }
    }

    internal class MessageReader
    {
      public const int SIZE_FIELD_SIZE = 4;
      public const int TYPE_FIELD_SIZE = 4;
      private byte[] _msg;
      private byte[] _size = new byte[SIZE_FIELD_SIZE];
      private byte[] _type = new byte[TYPE_FIELD_SIZE];
      private Stream _stream;
      private uint _max = 0;
      public MessageReader(Stream stream, uint max)
      {
        this._stream = stream;
        this._max = max;
      }

      public async Task<RawMessage> read()
      {
        await AsyncReadBuff(_stream, this._size);
        uint msgSize = BitConverter.ToUInt32(_size);
        if (msgSize > _max)
        {
          return null;
        }
        _msg = new byte[msgSize];
        await AsyncReadBuff(_stream, this._type);
        await AsyncReadBuff(_stream, this._msg);
        return new RawMessage()
        {
          buff = _msg,
          type = BitConverter.ToInt32(_type)
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

    public static Task SendMessage(Stream stream, IMessage message)
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