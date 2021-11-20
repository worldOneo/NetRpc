using System.Collections.Generic;
using System.Net.Sockets;
using System;

namespace NetRpc.Common
{
  public class DefaultContext : IContext
  {
    public Dictionary<string, object> Extras { get; set; } = new Dictionary<string, object>();
    public TcpClient client { get; set; }
    public Action<IMessage> respond { get; set; }
    public TcpClient Client() => client;
    public void Respond(IMessage msg) => respond(msg);
    public object Extra(string key) => Extras.GetValueOrDefault(key, null);
    public void Extra(string key, object value) => Extras[key] = value;

  }
}