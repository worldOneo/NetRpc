using System.Collections.Generic;

namespace NetRpc.Common
{
  public class MessageCoordinator : IFrameHandler
  {
    private Dictionary<int, IFrameHandler> _typedHandlers = new Dictionary<int, IFrameHandler>();

    public void Receive(IContext ctx, int type, byte[] data)
    {
      if (_typedHandlers[type] == null)
        return;
      _typedHandlers[type].Receive(ctx, type, data);
    }

    public void Register(int type, IFrameHandler handler)
    {
      _typedHandlers[type] = handler;
    }
  }
}