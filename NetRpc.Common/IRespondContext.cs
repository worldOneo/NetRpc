using System.Net.Sockets;

namespace NetRpc.Common
{
  public interface IRespondContext : IExtendable
  {
    void Respond(IMessage msg);
  }
}