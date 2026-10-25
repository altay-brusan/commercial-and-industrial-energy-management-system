using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System.Net.Http.Headers;

public class Program
{
    static async Task Main(string[] args)
    {
        //var input = new HelloRequest { Name = "sercobaba" };
        //var channel = GrpcChannel.ForAddress("https://localhost:7129");
        //var client = new Greeter.GreeterClient(channel);
        //var reply = await client.SayHelloAsync(input);
        //Console.WriteLine(reply.Message);

        var channel = GrpcChannel.ForAddress("https://localhost:7129");
        var customerClient = new Customer.CustomerClient(channel);

        var customerRequest = new CustomerLookUpModel { UserId=1 };

        var reply = await customerClient.GetCustomerInfoAsync(customerRequest);
        Console.WriteLine(reply);

        Console.WriteLine("-----------------------");
        using (var call = customerClient.GetNewCustomersInfo(new NewCustomerRequest()))
        {
            while (await call.ResponseStream.MoveNext())
            {
                var curentCustomer = call.ResponseStream.Current;   
                Console.WriteLine(curentCustomer);
            }
        }
        Console.WriteLine("-----------------------");

        var customerRequest2 = new NewCustomerRequest ();

        var reply2 = await customerClient.GetListCustomersAsync(customerRequest2);
        Console.WriteLine(reply2);
        Console.ReadLine();
    }
}