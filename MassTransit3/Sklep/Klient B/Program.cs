using System;
using System.Threading.Tasks;
using MassTransit;
using Sklep;

namespace KlientB
{
    static class Klient
    {
        public const string Nazwa = "KlientB";
    }

    class Message : IConsumer<IPytanieoPotwierdzenie>, IConsumer<IAkceptacjaZamowienia>, IConsumer<IOdrzucenieZamowienia>
    {
        public Task Consume(ConsumeContext<IPytanieoPotwierdzenie> context)
        {
            if (context.Message.Login == Klient.Nazwa)
            {
                Console.WriteLine($"Zaakceptować zamówienie {context.Message.CorrelationId}? T/N");
                bool czyZaakceptowac = Console.ReadKey().Key == ConsoleKey.T;

                Console.WriteLine();
                return Task.Run(() =>
                {
                    if (czyZaakceptowac)
                        context.RespondAsync(new Potwierdzenie() { CorrelationId = context.Message.CorrelationId });
                    else
                        context.RespondAsync(new BrakPotwierdzenia() { CorrelationId = context.Message.CorrelationId });
                });
            }
            else
                return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<IAkceptacjaZamowienia> context)
        {
            if (context.Message.Login == Klient.Nazwa)
                return Console.Out.WriteLineAsync($"Zamówienie {context.Message.CorrelationId} na ilość {context.Message.Ilosc} zaakceptowane.");
            else
                return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<IOdrzucenieZamowienia> context)
        {
            if (context.Message.Login == Klient.Nazwa)
                return Console.Out.WriteLineAsync($"Zamówienie {context.Message.CorrelationId} na ilość {context.Message.Ilosc} odrzucone.");
            else
                return Task.CompletedTask;
        }
    }

    class Program
    {
        static Task Main(string[] args)
        {
            var message = new Message();

            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"), h =>
                {
                    h.Username("auoyxxei");
                    h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok");
                });
                sbc.ReceiveEndpoint("klientb", ep => ep.Instance(message));
            });

            bus.Start();
            Console.WriteLine($"Zalogowano jako: {Klient.Nazwa}");

            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.K)
                {
                    Console.WriteLine("\nIle zamówić:");

                    int ilosc = 0;
                    try
                    {
                        ilosc = Convert.ToInt32(Console.ReadLine());
                        bus.Publish(new StartZamowienia() { Login = Klient.Nazwa, Ilosc = ilosc });
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Nie podano liczby!");
                    }
                }
            }

            bus.Stop();
        }
    }
}