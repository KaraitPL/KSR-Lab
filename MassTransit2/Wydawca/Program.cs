using System;
using System.Threading.Tasks;
using MassTransit;
using Wiadomosci;

namespace Wydawca
{

    internal class Program
    {
        static bool isRunning = false;

        class PublishObserver : IPublishObserver
        {
            public int postCounter = 0;
            public int preCounter = 0;
            public int faultCounter = 0;
            public Task PostPublish<T>(PublishContext<T> context) where T : class
            {
                return Task.Run(() => { postCounter++; });
            }

            public Task PrePublish<T>(PublishContext<T> context) where T : class
            {
                return Task.Run(() => { preCounter++; });
            }

            public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
            {
                return Task.Run(() => { faultCounter++; });
            }
        }

        class RecvObserver : IReceiveObserver
        {
            public int faultCounter = 0;
            public int postConsCounter = 0;
            public int postRecCounter = 0;
            public int preRecCounter = 0;
            public int faultRecCounter = 0;
            public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
            {
                return Task.Run(() => { faultCounter++; });
            }

            public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
            {
                return Task.Run(() => { postConsCounter++; });
            }

            public Task PostReceive(ReceiveContext context)
            {
                return Task.Run(() => { postRecCounter++; });
            }

            public Task PreReceive(ReceiveContext context)
            {
                return Task.Run(() => { preRecCounter++; });
            }

            public Task ReceiveFault(ReceiveContext context, Exception exception)
            {
                return Task.Run(() => { faultRecCounter++; });
            }
        }

        public class OdpAConsumer : IConsumer<OdpA>
        {

            public async Task Consume(ConsumeContext<OdpA> context)
            {
                Random random = new Random();
                if (random.Next(3) == 0)
                {
                    await context.RespondAsync(new ExcepA() { excep = "Wyjątek" });
                    Console.WriteLine("Exception u Consumer A");
                    throw new Exception($"Exception occured (OdpA)");
                }

                var response = context.Message;
                Console.WriteLine($"Przyszła odpowiedź od: {response.kto}");
            }

        }

        public class OdpBConsumer : IConsumer<OdpB>
        {
            int counter = 0;
            public async Task Consume(ConsumeContext<OdpB> context)
            {
                Random random = new Random();
                if (random.Next(3) == 0)
                {
                    await context.RespondAsync(new ExcepB() { excep = "Wyjątek" });
                    Console.WriteLine("Exception u Consumer B");
                    throw new Exception($"Exception occured (OdpB)");
                }
                var response = context.Message;
                Console.WriteLine($"Przyszła odpowiedź od: {response.kto}");
            }
        }

        static void Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"), h =>
                {
                    h.Username("auoyxxei");
                    h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok");
                });

                sbc.ReceiveEndpoint("odpqueueA", ep =>
                {
                    ep.Consumer<OdpAConsumer>();
                    ep.UseRetry(r => { r.Immediate(5); });
                });

                sbc.ReceiveEndpoint("odpqueueB", ep =>
                {
                    ep.Consumer<OdpBConsumer>();
                    ep.UseRetry(r => { r.Immediate(5); });
                });

                sbc.ReceiveEndpoint("ustaw", ep =>
                {
                    ep.Handler<Ustaw>(context =>
                    {
                        // Obsługa otrzymanego polecenia
                        var command = context.Message;
                        Console.WriteLine($"Received command: dziala = {command.dziala}");
                        isRunning = command.dziala;
                        return Task.CompletedTask;
                    });
                });
            });

            PublishObserver publishObserver = new PublishObserver();
            bus.ConnectPublishObserver(publishObserver);
            RecvObserver recvObserver = new RecvObserver();
            bus.ConnectReceiveObserver(recvObserver);

            bus.Start();
            Task.Run(() => SendMessage(bus, publishObserver, recvObserver));

            Console.WriteLine("Publisher is running. Press Ctrl+C to exit.");
            Console.ReadLine();
        }

        static async Task SendMessage(IBusControl bus, PublishObserver publishObserver, RecvObserver recvObserver)
        {
            int i = 0;
            bool show = true;
            while (true)
            {
                if (isRunning)
                {
                    show = true;
                    await bus.Publish(new Wiadomosc { wiadomosc = i });
                    Console.WriteLine($"Published message {i}");
                    i++;
                }
                else
                {
                    if (show == true)
                    {
                        Console.WriteLine("Statystyki:");
                        Console.WriteLine($"{recvObserver.preRecCounter} - Próba obsłużenia, {recvObserver.postRecCounter} - Liczba pomyślnie obsłużonych, {publishObserver.postCounter} - Liczba pomyślnie opublikowanych");
                        show = false;
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}