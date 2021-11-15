using NetRpc.Server;
using NetRpc.Common;
using System.Net;

namespace NetRpc.Demo
{
  public class RemoteServer
  {
    public RemoteServer()
    {
      var handler = new MessageCoordinator();
      var login = new MessageHandler<Login>(() => new Login(), LoginUser);
      handler.Register((int)MessageType.LOGIN, login);
      RpcServer server = new RpcServer(IPAddress.Loopback, 9000, handler);
    }

    public LoginResponse LoginUser(Context context, Login login)
    {
      return new LoginResponse()
      {
        successfull = login.username == "user" && login.password == "password"
      };
    }
  }
}