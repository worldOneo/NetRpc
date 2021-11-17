using NetRpc.Common;
using System.Net.Sockets;

namespace NetRpc.Client
{
  public class Sender : ISender<object>
  {
    public object Send(TcpClient client, IMessage message)
    {
      SendUtility.SendMessage(client.GetStream(), message);
      return null;
    }
  }
}