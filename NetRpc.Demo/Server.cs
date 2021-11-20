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

      // Error handling
      var errorGateHandler = ErrorGate.Create(keyInterceptor,
        (ctx, ex, type) => Console.WriteLine("[SERVER] Bad frame of type {0}:\n{1}\n{2} ", type, ex.Message, ex.StackTrace));

      var errorGateSender = ErrorGate.Create(keyInterceptor, (cl, ex, msg) =>
      {
        Console.WriteLine("[SERVER] Failed to write to {0} message of type {1}:\n{2}\n{3}",
          cl.Client.RemoteEndPoint, msg.Type(), ex.Message, ex.StackTrace);
        return Task.Run(() => null);
      });

      server = new RpcServer<Task>(IPAddress.Loopback, 9000, errorGateHandler, errorGateSender);
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
          await Task.Delay(1000);
          // Sending a bad message
          await SendUtility.SendMessage(context.Client().GetStream(), new Alive());
        });
      return new LoginResponse()
      {
        Successful = loggedIn
      };
    }
  }
}