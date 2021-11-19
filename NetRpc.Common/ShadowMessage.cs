using System;

namespace NetRpc.Common
{
  public class ShadowMessage : IMessage
  {
    private int _type;
    private Guid _guid;
    private byte[] _data;

    public ShadowMessage(int type, Guid guid, byte[] data)
    {
      _type = type;
      _guid = guid;
      _data = data;
    }

    public int Type() => _type;
    public Guid GetGuid() => _guid;
    public byte[] Encode() => _data;
    public void SetGuid(Guid guid) => throw new NotSupportedException("ShadowMessage cant be changed");
    public void Decode(byte[] data) => throw new NotSupportedException("ShadowMessage cant be decoded");
  }
}