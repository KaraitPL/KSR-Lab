using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;

namespace AbonentA
{
    internal class Program
    {
        public static Task Handle(ConsumeContext<Wiadomosc> ctx)
        {
            if(ctx.Message.wiadomosc % 2 == 0)
            {
                ctx.RespondAsync(new OdpA() { kto = "abonent A" });
            }
            String output = "Wiadomość: " + ctx.Message.wiadomosc;
            return Console.Out.WriteLineAsync(output);
        }

        public static Task Handle(ConsumeContext<ExcepA> ctx)
        {
            String output = "Pojawił się wyjątek: " + ctx.Message.excep;
            return Console.Out.WriteLineAsync(output);
        }

        static void Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"),
                h => { h.Username("auoyxxei"); h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok"); });
                sbc.ReceiveEndpoint("recvqueue", ep => {
                    ep.Handler<Wiadomosc>(Handle);
                    ep.Handler<ExcepA>(Handle);
                });
            });

            Console.WriteLine("Odbiorca A");
            bus.Start();
            Console.ReadKey();
            bus.Stop();
        }
    }
}
