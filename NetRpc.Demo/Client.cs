using System;
using System.Net;
using NetRpc.Client;
using NetRpc.Common;
using System.Threading;
using System.Threading.Tasks;
namespace NetRpc.Demo
{
  class Program
  {
    static void Main(string[] args)
    {
      ThreadPool.QueueUserWorkItem(b =>
      {
        new RemoteServer();
        Console.WriteLine("Server thread goes bye bye");
      });
      test();
      Console.ReadLine();
    }

    async static void test()
    {
      var coordinator = new FulfillingCoordinator();
      coordinator.Register(
        (int)MessageType.ALIVE,
        new ReceiverCallback((ctx, msg) => Console.WriteLine("[Client] Independent alive received"))
      );

      coordinator.RegisterMessageFactory((int)MessageType.LOGIN, () => new Login());
      coordinator.RegisterMessageFactory((int)MessageType.LOGIN_RESPONSE, () => new LoginResponse());
      coordinator.RegisterMessageFactory((int)MessageType.ALIVE, () => new Alive());


      var client = new RpcClient<Task<IMessage>>(IPAddress.Loopback, 9000, coordinator, coordinator);

      Console.Write("Password: ");
      var argument = new Login()
      {
        Username = "user",
        Password = Console.ReadLine()
      };

      LoginResponse success = (LoginResponse)await client.SendMessage(argument);

      Console.WriteLine("[Client] " + (success.Successful ? "Logged in" : "Loggin failed"));
    }
  }
}
