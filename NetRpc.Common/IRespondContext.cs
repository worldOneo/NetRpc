using System.Net.Sockets;

namespace NetRpc.Common
{
  public interface IRespondContext
  {
    void Respond(IMessage msg);
  }
}