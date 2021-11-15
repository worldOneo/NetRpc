using System.Collections.Generic;

namespace NetRpc.Common
{
  public class MessageCoordinator : FrameHandler
  {
    private Dictionary<int, FrameHandler> typedHandlers = new Dictionary<int, FrameHandler>();

    public void Receive(Context ctx, int type, byte[] data)
    {
      if (typedHandlers[type] == null)
        return;
      typedHandlers[type].Receive(ctx, type, data);
    }

    public void Register(int type, FrameHandler handler)
    {
      typedHandlers[type] = handler;
    }
  }
}