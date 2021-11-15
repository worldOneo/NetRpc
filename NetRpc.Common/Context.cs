using System.Net.Sockets;

namespace NetRpc.Common
{
  public interface Context
  {
    TcpClient Client();
    void Respond(Message msg);
  }
}