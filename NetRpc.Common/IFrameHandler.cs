namespace NetRpc.Common
{
  public interface IFrameHandler
  {
    void Receive(IContext ctx, int type, byte[] data);
  }
}