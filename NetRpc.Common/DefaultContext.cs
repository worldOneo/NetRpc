using System.Net.Sockets;
using System;

namespace NetRpc.Common
{
  public class DefaultContext : IContext
  {
    public TcpClient client { get; set; }
    public Action<IMessage> respond { get; set; }
    public TcpClient Client() => client;
    public void Respond(IMessage msg) => respond(msg);

  }
}