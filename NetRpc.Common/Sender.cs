using NetRpc.Common;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetRpc.Common
{
  public class Sender : ISender<Task>
  {
    public Task Send(TcpClient client, IMessage message)
    {
      return SendUtility.SendMessage(client.GetStream(), message); ;
    }
  }
}