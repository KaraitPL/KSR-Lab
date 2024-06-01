using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wiadomosci;
using MassTransit;

namespace Wydawca
{
    public class Wiadomosc : IWiadomosc1
    {
        public int temperatura { get; set; }
    }

    public class Wiadomosc2 : IWiadomosc2
    {
        public string wiadomosc2 { get; set; }
    }

    internal class Program
    {


        static void Main(string[] args)
        {
            Random random = new Random();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"),
                h => { h.Username("auoyxxei"); h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok"); });
            });
            bus.Start();
            for (int i = 0; i < 10; i++)
            {
                bus.Publish(new Wiadomosc() { temperatura = random.Next(10, 20) }, ctx =>
                {
                    ctx.Headers.Set("Nadawca", "Program1");
                    ctx.Headers.Set("Odbiorca", "Program2");
                });
            }

            for (int i = 0; i < 10; i++)
            {
                bus.Publish(new Wiadomosc2() { wiadomosc2 = "Wiadomość 2" }, ctx =>
                {
                    ctx.Headers.Set("NadawcaZad2", "Program1Zad2");
                    ctx.Headers.Set("OdbiorcaZad2", "Program2Zad2");
                });
            }
            Console.ReadKey();
            bus.Stop();
        }
    }
}