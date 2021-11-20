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
    public const int IV_SIZE = 16;
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
      var tag = new byte[KEY_SIZE];
      byte[] payload;
      using (var random = RandomNumberGenerator.Create())
      {
        random.GetBytes(iv);
        using (var cipher = AesManaged.Create())
        {
          cipher.KeySize = 256;
          cipher.Key = _keyStore.Get(client);
          cipher.IV = iv;
          cipher.Mode = CipherMode.CBC;
          cipher.Padding = PaddingMode.Zeros;
          var encryptor = cipher.CreateEncryptor(cipher.Key, cipher.IV);
          using (MemoryStream msEncrypt = new MemoryStream())
          {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
              csEncrypt.Write(data);
            }
            payload = msEncrypt.ToArray();
          }
        }
      }
      var encrypted = new byte[payload.Length + IV_SIZE];
      Array.Copy(iv, encrypted, IV_SIZE);
      Array.Copy(payload, 0, encrypted, IV_SIZE, payload.Length);
      return _sender.Send(client, new ShadowMessage(message.Type(), message.GetGuid(), encrypted));
    }

    public void Receive(IContext ctx, int type, byte[] bytes)
    {
      var iv = new byte[IV_SIZE];
      var tag = new byte[KEY_SIZE];
      Array.Copy(bytes, iv, IV_SIZE);
      var payloadSize = bytes.Length - IV_SIZE;
      var payload = new byte[payloadSize];
      byte[] decrypted = new byte[payloadSize];
      Array.Copy(bytes, IV_SIZE, payload, 0, payloadSize);
      using (var cipher = AesManaged.Create())
      {
        cipher.KeySize = 256;
        cipher.Key = _keyStore.Get(ctx.Client());
        cipher.IV = iv;
        cipher.Mode = CipherMode.CBC;
        cipher.Padding = PaddingMode.Zeros;
        var decryptor = cipher.CreateDecryptor(cipher.Key, cipher.IV);
        using (MemoryStream msDecrypt = new MemoryStream(payload))
        {
          using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            csDecrypt.Read(decrypted);
        }
        _handler.Receive(ctx, type, decrypted);
      }
    }
  }
}