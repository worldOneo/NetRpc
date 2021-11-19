using System;
using System.IO;

namespace NetRpc.Common
{
  public abstract class IdMessage : IMessage
  {
    private Guid _id = Guid.NewGuid();
    public Guid GetGuid() => _id;
    public void SetGuid(Guid id) => this._id = id;

    protected byte[] Encode(byte[] payload)
    {
      var stream = new MemoryStream();
      var writer = new BinaryWriter(stream);
      writer.Write(_id.ToByteArray());
      writer.Write(payload);
      writer.Flush();
      return stream.GetBuffer();
    }

    protected void Decode(BinaryReader reader)
    {
      var id = reader.ReadBytes(16);
      this._id = new Guid(id);
    }
    public abstract byte[] Encode();
    public abstract void Decode(byte[] data);
    public abstract int Type();
  }
}