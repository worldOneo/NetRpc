using System;

namespace NetRpc.Common
{
  public class MessageHandler<T> : IFrameHandler where T : IMessage
  {
    public Func<T> Factory { get; set; }
    public Func<IContext, T, IMessage> Callback { get; set; }

    public MessageHandler(Func<T> factory, Func<IContext, T, IMessage> callback)
    {
      Factory = factory;
      Callback = callback;
    }

    public void Receive(IContext ctx, int type, byte[] data)
    {
      var raw = Factory();
      raw.Decode(data);
      var resp = Callback(ctx, raw);
      resp.SetGuid(raw.GetGuid());
      ctx.Respond(resp);
    }
  }
}