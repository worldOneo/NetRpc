using System;
using System.Net.Sockets;

namespace NetRpc.Common
{
  public abstract class ErrorGate
  {
    public static FrameHandlerErrorGate Create(IFrameHandler handler, Action<IContext, Exception, int> error)
      => new FrameHandlerErrorGate(handler, error);
    public static SendingErrorGate<T> Create<T>(ISender<T> sender, Func<TcpClient, Exception, IMessage, T> error)
      => new SendingErrorGate<T>(sender, error);
  }

  public class FrameHandlerErrorGate : ErrorGate, IFrameHandler
  {
    private IFrameHandler _next;
    private Action<IContext, Exception, int> _errorHandler;
    public FrameHandlerErrorGate(IFrameHandler next, Action<IContext, Exception, int> error)
    {
      _next = next;
      _errorHandler = error;
    }

    public void Receive(IContext ctx, int type, byte[] data)
    {
      try
      {
        _next.Receive(ctx, type, data);
      }
      catch (Exception e)
      {
        _errorHandler(ctx, e, type);
      }
    }
  }

  public class SendingErrorGate<T> : ErrorGate, ISender<T>
  {
    private ISender<T> _next;
    private Func<TcpClient, Exception, IMessage, T> _errorHandler;
    public SendingErrorGate(ISender<T> next, Func<TcpClient, Exception, IMessage, T> error)
    {
      _next = next;
      _errorHandler = error;
    }

    public T Send(TcpClient client, IMessage message)
    {
      try
      {
        return _next.Send(client, message);
      }
      catch (Exception e)
      {
        return _errorHandler(client, e, message);
      }
    }
  }
}