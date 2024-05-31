using System;
using System.Threading.Tasks;
using MassTransit;
using Sklep;

namespace Magazyn
{
    class Magazyn : IConsumer<IPytanieoWolne>, IConsumer<IAkceptacjaZamowienia>, IConsumer<IOdrzucenieZamowienia>
    {
        public int Wolne { get; set; } = 0;
        public int Zarezerwowane { get; set; } = 0;

        public Task Consume(ConsumeContext<IPytanieoWolne> context)
        {
            return Task.Run(() =>
            {
                if (Wolne >= context.Message.Ilosc)
                {
                    Wolne -= context.Message.Ilosc;
                    Zarezerwowane += context.Message.Ilosc;
                    Console.Out.WriteLineAsync($"Można zrealizować zamówienie {context.Message.CorrelationId} na ilość {context.Message.Ilosc}");
                    context.RespondAsync(new OdpowiedzWolne() { CorrelationId = context.Message.CorrelationId });
                }
                else
                {
                    Zarezerwowane += context.Message.Ilosc;
                    Wolne -= context.Message.Ilosc;
                    Console.Out.WriteLineAsync($"Brak wystarczającej liczby produktów do zamówienia {context.Message.CorrelationId} na ilość {context.Message.Ilosc}");
                    context.RespondAsync(new OdpowiedzWolneNegatywna() { CorrelationId = context.Message.CorrelationId });
                }
            });
        }

        public Task Consume(ConsumeContext<IAkceptacjaZamowienia> context)
        {
            return Task.Run(() =>
            {
                Zarezerwowane -= context.Message.Ilosc;
                Console.WriteLine($"Zamówienie zrealizowane: {context.Message.CorrelationId} na ilość {context.Message.Ilosc}. Przesyłka wysłana do klienta.");
            });
        }

        public Task Consume(ConsumeContext<IOdrzucenieZamowienia> context)
        {
            return Task.Run(() =>
            {
                Zarezerwowane -= context.Message.Ilosc;
                Wolne += context.Message.Ilosc;
                Console.WriteLine($"Zamówienie odrzucone: {context.Message.CorrelationId} na ilość {context.Message.Ilosc}. Produkty ponownie dostępne do sprzedaży.");
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var magazyn = new Magazyn();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"), h =>
                {
                    h.Username("auoyxxei");
                    h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok");
                });
                sbc.ReceiveEndpoint("magazyn", ep => ep.Instance(magazyn));
            });

            Console.WriteLine("Magazyn");
            bus.Start();

            int liczbaDostarczonychProduktow = 0;

            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.M)
                {
                    Console.WriteLine("\nPodaj liczbę produktów:");
                    try
                    {
                        liczbaDostarczonychProduktow = Convert.ToInt32(Console.ReadLine());
                        magazyn.Wolne += liczbaDostarczonychProduktow;
                        Console.WriteLine($"\nStan: \nDostawa: {liczbaDostarczonychProduktow} Wolne: {magazyn.Wolne} Zarezerwowane: {magazyn.Zarezerwowane}");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Nie podano liczby! Operacja anulowana.");
                    }
                }
            }
            bus.Stop();
        }
    }
}