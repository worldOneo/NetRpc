using System;
using System.Net;
using NetRpc.Client;

namespace NetRpc.Demo
{
  class Program
  {
    static void Main(string[] args)
    {
      test();
      Console.ReadLine();
    }

    async static void test()
    {
      RpcClient client = new RpcClient(IPAddress.Loopback, 9000);
      client.RegisterMessageFactory((int)MessageType.LOGIN, () => new Login());
      client.RegisterMessageFactory((int)MessageType.LOGIN_RESPONSE, () => new LoginResponse());

      Console.Write("Password: ");
      var argument = new Login()
      {
        username = "juun",
        password = Console.ReadLine()
      };

      LoginResponse success = (LoginResponse)await client.SendMessage(argument);

      Console.WriteLine(success.successfull ? "Logged in" : "Loggin failed");
    }
  }
}
