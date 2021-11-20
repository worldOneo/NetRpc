using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.Sockets;
using System;
using System.IO;

namespace NetRpc.Common.Secruity
{
  public abstract class Encryptor
  {
    public const int KEY_SIZE = 32;
    public const int IV_SIZE = 12;
    public const int TAG_SIZE = 16;
    public static Encryptor<Task> Create(IFrameHandler handler, IKeyStore<TcpClient> keyStore)
      => new Encryptor<Task>(handler, keyStore, new Sender());
  }

  public class Encryptor<T> : Encryptor, ISender<T>, IFrameHandler
  {
    private IFrameHandler _handler;
    private ISender<T> _sender;
    private IKeyStore<TcpClient> _keyStore;

    public Encryptor(IFrameHandler handler, IKeyStore<TcpClient> keyStore, ISender<T> sender)
    {
      _handler = handler;
      _sender = sender;
      _keyStore = keyStore;
    }

    public T Send(TcpClient client, IMessage message)
    {
      var data = message.Encode();
      var iv = new byte[IV_SIZE];
      var tag = new byte[TAG_SIZE];
      var payload = new byte[data.Length];
      using (var random = RandomNumberGenerator.Create())
      {
        random.GetBytes(iv);
        using (var cipher = new AesGcm(_keyStore.Get(client)))
        {
          cipher.Encrypt(iv, data, payload, tag);
        }
      }
      var encrypted = new byte[payload.Length + IV_SIZE + TAG_SIZE];
      Array.Copy(iv, encrypted, IV_SIZE);
      Array.Copy(tag, 0, encrypted, IV_SIZE, TAG_SIZE);
      Array.Copy(payload, 0, encrypted, IV_SIZE + TAG_SIZE, payload.Length);
      return _sender.Send(client, new ShadowMessage(message.Type(), message.GetGuid(), encrypted));
    }

    public void Receive(IContext ctx, int type, byte[] bytes)
    {
      var iv = new byte[IV_SIZE];
      var tag = new byte[TAG_SIZE];
      var payloadSize = bytes.Length - IV_SIZE - TAG_SIZE;
      var payload = new byte[payloadSize];
      Array.Copy(bytes, iv, IV_SIZE);
      Array.Copy(bytes, IV_SIZE, tag, 0, TAG_SIZE);
      Array.Copy(bytes, IV_SIZE + TAG_SIZE, payload, 0, payloadSize);
      byte[] decrypted = new byte[payloadSize];
      using (var cipher = new AesGcm(_keyStore.Get(ctx.Client())))
      {
        cipher.Decrypt(iv, payload, tag, decrypted);
      }
      _handler.Receive(ctx, type, decrypted);
    }
  }
}