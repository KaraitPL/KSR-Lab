using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;

namespace AbonentB
{
    internal class Program
    {
        public static Task Handle(ConsumeContext<Wiadomosc> ctx)
        {
            if(ctx.Message.wiadomosc % 3 == 0)
            {
                ctx.RespondAsync(new OdpB() { kto = "abonent B" });
            }
            String output = "Wiadomość: " + ctx.Message.wiadomosc;
            return Console.Out.WriteLineAsync(output);
        }

        public static Task Handle(ConsumeContext<ExcepB> ctx)
        {
            String output = "Pojawił się wyjątek: " + ctx.Message.excep;
            return Console.Out.WriteLineAsync(output);
        }

        static void Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"),
                h => { h.Username("auoyxxei"); h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok"); });
                sbc.ReceiveEndpoint("recvqueue2", ep => {
                    ep.Handler<Wiadomosc>(Handle);
                    ep.Handler<ExcepB>(Handle);
                });
            });

            Console.WriteLine("Odbiorca B");
            bus.Start();
            Console.ReadKey();
            bus.Stop();
        }
    }
}
