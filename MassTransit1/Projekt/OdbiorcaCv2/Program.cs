using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;
using static MassTransit.MessageHeaders;
using System.Runtime.Remoting.Contexts;

namespace OdbiorcaCv2
{

    internal class Program
    {
        public static Task Handle(ConsumeContext<IWiadomosc2> ctx)
        {
            String output = "";
            foreach (var hdr in ctx.Headers.GetAll())
            {
                output += hdr.Key + ": " + hdr.Value + "\n";
            }
            output += "Zadanie 5: " + ctx.Message.wiadomosc2;
            return Console.Out.WriteLineAsync(output);
        }



        static void Main(string[] args)
        {
            Random random = new Random();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"),
                h => { h.Username("auoyxxei"); h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok"); });
                sbc.ReceiveEndpoint("recvqueueC", ep => {
                    ep.Handler<IWiadomosc2>(Handle);
                });
            });

            bus.Start();
            Console.ReadKey();
            bus.Stop();
        }
    }
}
