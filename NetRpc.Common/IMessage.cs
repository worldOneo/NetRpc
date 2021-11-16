using System;

namespace NetRpc.Common
{
  public interface IMessage
  {
    Guid GetGuid();
    void SetGuid(Guid id);
    int Type();
    byte[] Encode();
    void Decode(byte[] data);
  }
}
