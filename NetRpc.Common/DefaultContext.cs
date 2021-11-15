using System.Net.Sockets;
using System;

namespace NetRpc.Common
{
  public class DefaultContext : Context
  {
    public TcpClient client { get; set; }
    public Action<Message> respond { get; set; }
    public TcpClient Client() => client;
    public void Respond(Message msg) => respond(msg);

  }
}