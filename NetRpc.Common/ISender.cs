using System.Net.Sockets;

namespace NetRpc.Common
{
  public interface ISender<T>
  {
    T Send(TcpClient client, IMessage Message);
  }
}