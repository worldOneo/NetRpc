using System.Collections.Generic;
using NetRpc.Common;

namespace NetRpc.Server
{
  public class MessageCoordinator : IFrameHandler
  {
    private Dictionary<int, IFrameHandler> _typedHandlers = new Dictionary<int, IFrameHandler>();

    public void Receive(IContext ctx, int type, byte[] data)
    {
      if (_typedHandlers[type] == null)
        return;
      _typedHandlers[type].Receive(ctx, type, data);
      return;
    }

    public void Register(int type, IFrameHandler handler)
    {
      _typedHandlers[type] = handler;
    }
  }
}