using NetRpc.Common;
using NetRpc.Common.Secruity;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace NetRpc.Demo
{
  public class KeyExchangeInterceptor<T> : Interceptor<T>
  {
    private MapKeyStore<TcpClient> _store;
    private ISender<T> _sender;
    private RSACryptoServiceProvider _rsa;
    public KeyExchangeInterceptor(IFrameHandler next, ISender<T> nextSender, MapKeyStore<TcpClient> store, ISender<T> sender, RSACryptoServiceProvider rsa)
      : base(next, null, nextSender, null)
    {
      this._handlerInt = ReceiveKey;
      this._store = store;
      this._senderInt = SendKey;
      this._sender = sender;
      this._rsa = rsa;
    }

    public T SendKey(TcpClient client, IMessage message)
    {
      if (message.Type() != (int)MessageType.KEY_EXCHANGE)
        return default(T);
      var encrypted = _rsa.Encrypt(message.Encode(), false);
      return _sender.Send(client, new ShadowMessage(message.Type(), message.GetGuid(), encrypted));
    }

    public bool ReceiveKey(IContext context, int type, byte[] data)
    {
      if (type != (int)MessageType.KEY_EXCHANGE)
        return true;
      var message = new KeyExchange();
      message.Decode(_rsa.Decrypt(data, false));
      _store.Add(context.Client(), message.Key);
      return false;
    }
  }
}