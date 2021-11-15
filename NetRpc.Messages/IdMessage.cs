using NetRpc.Common;
using System;
using System.IO;

namespace NetRpc.Messages
{
  public abstract class IdMessage : Message
  {
    private Guid id = Guid.NewGuid();
    public Guid GetGuid() => id;
    public void SetGuid(Guid id) => this.id = id;

    protected byte[] Encode(byte[] payload)
    {
      var stream = new MemoryStream();
      var writer = new BinaryWriter(stream);
      writer.Write(id.ToByteArray());
      writer.Write(payload);
      writer.Flush();
      return stream.GetBuffer();
    }

    protected void Decode(BinaryReader reader)
    {
      var id = reader.ReadBytes(16);
      this.id = new Guid(id);
    }
    public abstract byte[] Encode();
    public abstract void Decode(byte[] data);
    public abstract int Type();
  }
}