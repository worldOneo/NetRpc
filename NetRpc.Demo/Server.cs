using NetRpc.Server;
using NetRpc.Common;
using System.Net;
using System;

namespace NetRpc.Demo
{
  public class RemoteServer
  {
    private RpcServer server;
    public RemoteServer()
    {
      var handler = new MessageCoordinator();
      var login = new MessageHandler<Login>(() => new Login(), LoginUser);
      handler.Register((int)MessageType.LOGIN, login);
      server = new RpcServer(IPAddress.Loopback, 9000, handler);
      server.ErrorHandler = err => Console.WriteLine("[Server] Error: " + err.StackTrace);
      server.Start().Wait();
    }

    public void Stop() => server.Stop();

    public LoginResponse LoginUser(IContext context, Login login)
    {
      Console.WriteLine("[Server] Loggin attempt: " + login.username + "@" + login.password);
      return new LoginResponse()
      {
        successfull = login.username == "user" && login.password == "password"
      };
    }
  }
}