using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wiadomosci;
using MassTransit;
using static MassTransit.MessageHeaders;
using System.Runtime.Remoting.Contexts;


namespace OdbiorcaBv2
{

    public class Handler : IConsumer<IWiadomosc1>
    {
        int counter = 0;
        public Task Consume(ConsumeContext<IWiadomosc1> ctx)
        {
            
            String output = "";
            foreach (var hdr in ctx.Headers.GetAll())
            {
                output += hdr.Key + ": " + hdr.Value + "\n";
            }
            output += "Temperatura: " + ctx.Message.temperatura;
            return Console.Out.WriteLineAsync(output + " Wiadomość nr: " + ++counter);
        }
    }

    internal class Program
    {
        public static Task Handle(ConsumeContext<IWiadomosc1> ctx)
        {
            String output = "";
            foreach (var hdr in ctx.Headers.GetAll())
            {
                output += hdr.Key + ": " + hdr.Value + "\n";
            }
            output += "Temperatura: " + ctx.Message.temperatura;
            return Console.Out.WriteLineAsync(output);
        }
        static void Main(string[] args)
        {
            var instance = new Handler();

            Random random = new Random();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"),
                h => { h.Username("auoyxxei"); h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok"); });
                sbc.ReceiveEndpoint("recvqueue", ep => {
                    ep.Instance(instance);
                });
            });

            bus.Start();
            Console.ReadKey();
            bus.Stop();
        }
    }
}
