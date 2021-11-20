using NetRpc.Common;
using NetRpc.Client;
using NetRpc.Common.Secruity;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace NetRpc.Demo
{
  public class RemoteClient
  {
    private RpcClient<Task<IMessage>> _client;
    public RemoteClient(RSACryptoServiceProvider pub)
    {
      var coordinator = new FulfillingCoordinator();
      coordinator.Register(
        (int)MessageType.ALIVE,
        new ReceiverCallback((ctx, msg) => Console.WriteLine("[Client] Independent alive received"))
      );

      coordinator.RegisterMessageFactory((int)MessageType.LOGIN, () => new Login());
      coordinator.RegisterMessageFactory((int)MessageType.LOGIN_RESPONSE, () => new LoginResponse());
      coordinator.RegisterMessageFactory((int)MessageType.ALIVE, () => new Alive());

      // Crypto
      var keyStore = new MapKeyStore<TcpClient>();
      var encription = new Encryptor<Task<IMessage>>(coordinator, keyStore, coordinator);
      var keyInterceptor = new KeyExchangeInterceptor<Task<IMessage>>(encription, encription, keyStore, coordinator, pub);

      _client = new RpcClient<Task<IMessage>>(IPAddress.Loopback, 9000, keyInterceptor, keyInterceptor);

      // Symmetric key exchange over RSA
      var key = new byte[Encryptor.KEY_SIZE];
      RandomNumberGenerator.Create().GetBytes(key);
      keyStore.Add(_client.Client(), key);
      var keyExchange = new KeyExchange() { Key = key };
      _client.SendMessage(keyExchange);
    }

    public async Task<LoginResponse> Login(string username, string password)
    {
      return (LoginResponse)await _client.SendMessage(new Login()
      {
        Username = username,
        Password = password,
      });
    }
  }
}