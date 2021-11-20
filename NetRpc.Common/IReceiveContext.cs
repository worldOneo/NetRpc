using System.Net.Sockets;

namespace NetRpc.Common
{
  public interface IReceiveContext : IExtendable
  {
    TcpClient Client();
  }
}