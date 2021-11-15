namespace NetRpc.Common
{
  public interface FrameHandler
  {
    void Receive(Context ctx, int type, byte[] data);
  }
}