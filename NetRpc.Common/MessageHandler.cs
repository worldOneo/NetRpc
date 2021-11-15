using System;

namespace NetRpc.Common
{
  public class MessageHandler<T> : FrameHandler where T : Message
  {
    public Func<T> Factory { get; set; }
    public Func<Context, T, Message> Callback { get; set; }

    public MessageHandler(Func<T> factory, Func<Context, T, Message> callback)
    {
      Factory = factory;
      Callback = callback;
    }

    public void Receive(Context ctx, int type, byte[] data)
    {
      var raw = Factory();
      raw.Decode(data);
      var resp = Callback(ctx, raw);
      resp.SetGuid(raw.GetGuid());
      ctx.Respond(resp);
    }
  }
}