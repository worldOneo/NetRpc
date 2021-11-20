using System.Collections.Generic;

namespace NetRpc.Common.Secruity
{
  public class MapKeyStore<T> : IKeyStore<T>
  {
    private Dictionary<T, byte[]> _dict = new Dictionary<T, byte[]>();
    public byte[] Get(T client) => _dict.ContainsKey(client) ? _dict[client] : null;
    public void Add(T client, byte[] key) => _dict[client] = key;
    public void Remove(T client) => _dict.Remove(client);
  }
}