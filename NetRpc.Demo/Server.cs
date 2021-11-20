using NetRpc.Server;
using NetRpc.Common;
using NetRpc.Common.Secruity;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace NetRpc.Demo
{
  public class RemoteServer
  {
    private RpcServer<Task> server;
    public RemoteServer(RSACryptoServiceProvider priv)
    {
      var handler = new MessageCoordinator();
      var login = new MessageHandler<Login>(() => new Login(), LoginUser);
      handler.Register((int)MessageType.LOGIN, login);

      // Crypto
      var keyStore = new MapKeyStore<TcpClient>();
      var cryptor = Encryptor.Create(handler, keyStore);
      var keyInterceptor = new KeyExchangeInterceptor<Task>(cryptor, cryptor, keyStore, new Sender(), priv);

      server = new RpcServer<Task>(IPAddress.Loopback, 9000, keyInterceptor, keyInterceptor);
      server.Disconnect = c => keyStore.Remove(c);
      server.ErrorHandler = err => Console.WriteLine("[Server] Error: {0}\n{1}", err.Message, err.StackTrace);
      server.Start().Wait();
    }

    public void Stop() => server.Stop();

    public LoginResponse LoginUser(IContext context, Login login)
    {
      Console.WriteLine("[Server] Login attempt: " + login.Username + "@" + login.Password);
      var loggedIn = login.Username == "user" && login.Password == "password";
      if (loggedIn)
        Task.Run(async () =>
        {
          await Task.Delay(1000);
          context.Respond(new Alive());
        });
      return new LoginResponse()
      {
        Successful = loggedIn
      };
    }
  }
}