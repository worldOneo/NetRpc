using System;
using System.Net;
using NetRpc.Client;
using NetRpc.Common;
using System.Threading;
using System.Net.Sockets;
using NetRpc.Common.Secruity;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace NetRpc.Demo
{
  class Program
  {
    static void Main(string[] args)
    {
      var rsa = new RSACryptoServiceProvider(4096);
      var pub = new RSACryptoServiceProvider();
      var priv = new RSACryptoServiceProvider();
      pub.ImportParameters(rsa.ExportParameters(false));
      priv.ImportParameters(rsa.ExportParameters(true));

      ThreadPool.QueueUserWorkItem(b =>
      {
        new RemoteServer(priv);
        Console.WriteLine("Server thread goes bye bye");
      });
      test(pub);
      Console.ReadLine();
    }

    async static void test(RSACryptoServiceProvider pub)
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

      var client = new RpcClient<Task<IMessage>>(IPAddress.Loopback, 9000, keyInterceptor, keyInterceptor);
      Console.Write("Password: ");
      var argument = new Login()
      {
        Username = "user",
        Password = "password"
      };

      var key = new byte[Encryptor.KEY_SIZE];
      RandomNumberGenerator.Create().GetBytes(key);
      keyStore.Add(client.Client(), key);
      var keyExchange = new KeyExchange() { Key = key };
      client.SendMessage(keyExchange);
      LoginResponse success = (LoginResponse)await client.SendMessage(argument);

      Console.WriteLine("[Client] " + (success.Successful ? "Logged in" : "Loggin failed"));
    }
  }
}
