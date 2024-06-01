using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;
using static MassTransit.MessageHeaders;
using System.Runtime.Remoting.Contexts;

namespace OdbiorcaC
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
        public static Task Handle(ConsumeContext<Wiadomosc2> ctx)
        {
            String output = "";
            foreach (var hdr in ctx.Headers.GetAll())
            {
                output += hdr.Key + ": " + hdr.Value + "\n";
            }
            output += "Temperatura: " + ctx.Message.wiadomosc2;
            return Console.Out.WriteLineAsync(output);
        }



        static void Main(string[] args)
        {
            Random random = new Random();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"),
                h => { h.Username("auoyxxei"); h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok"); });
                sbc.ReceiveEndpoint("recvqueue", ep => {
                    ep.Handler<Wiadomosc2>(Handle);
                });
            });

            bus.Start();
            Console.ReadKey();
            bus.Stop();
        }
    }
}
