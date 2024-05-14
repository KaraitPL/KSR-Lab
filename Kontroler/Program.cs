using System;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;

namespace Kontroler
{

    internal class Program
    {
        static Task Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"), h =>
                {
                    h.Username("auoyxxei");
                    h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok");
                });
            });

            bus.Start();

            Console.WriteLine("Controller is running. Press 's' to start, 't' to stop, and 'q' to quit.");

            while (true)
            {
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.S:
                        bus.Publish(new Ustaw { dziala = true });
                        Console.WriteLine("Sent start command");
                        break;
                    case ConsoleKey.T:
                        bus.Publish(new Ustaw { dziala = false });
                        Console.WriteLine("Sent stop command");
                        break;
                    case ConsoleKey.Q:
                        bus.Stop();
                        break;
                }
            }
        }
    }
}