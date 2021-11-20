using System.Net.Sockets;

namespace NetRpc.Common.Secruity
{
  public interface IKeyStore<T>
  {
    byte[] Get(T client);
  }
}