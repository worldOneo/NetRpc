using System.Net.Sockets;

namespace NetRpc.Common
{
  public interface IContext
  {
    TcpClient Client();
    void Respond(IMessage msg);
  }
}