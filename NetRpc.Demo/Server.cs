using NetRpc.Server;
using NetRpc.Common;
using System.Net;
using System;
using System.Threading.Tasks;

namespace NetRpc.Demo
{
  public class RemoteServer
  {
    private RpcServer<Task> server;
    public RemoteServer()
    {
      var handler = new MessageCoordinator();
      var login = new MessageHandler<Login>(() => new Login(), LoginUser);
      handler.Register((int)MessageType.LOGIN, login);
      var cryptor = Encryptor.Create(handler);
      server = new RpcServer<Task>(IPAddress.Loopback, 9000, cryptor, cryptor);
      server.ErrorHandler = err => Console.WriteLine("[Server] Error: " + err.StackTrace);
      server.Start().Wait();
    }

    public void Stop() => server.Stop();

    public LoginResponse LoginUser(IContext context, Login login)
    {
      Console.WriteLine("[Server] Loggin attempt: " + login.Username + "@" + login.Password);
      Task.Run(async () =>
      {
        await Task.Delay(1000);
        await SendUtility.SendMessage(context.Client().GetStream(), new Alive());
      });
      return new LoginResponse()
      {
        Successful = login.Username == "user" && login.Password == "password"
      };
    }
  }
}