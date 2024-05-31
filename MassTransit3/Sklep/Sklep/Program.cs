using System;
using MassTransit;
using MassTransit.Saga;

namespace Sklep
{
    public class Zamowienie : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string Login { get; set; }
        public int Ilosc { get; set; }
        public Guid? TimeoutId { get; set; }
    }

    public class Sklep : MassTransitStateMachine<Zamowienie>
    {
        public State Niepotwierdzone { get; private set; }
        public State PotwierdzoneKlient { get; private set; }
        public State PotwierdzoneMagazyn { get; private set; }

        public Event<StartZamowienia> StartZamowienia { get; private set; }
        public Event<Potwierdzenie> Potwierdzenie { get; private set; }
        public Event<BrakPotwierdzenia> BrakPotwierdzenia { get; private set; }
        public Event<OdpowiedzWolne> OdpowiedzWolne { get; private set; }
        public Event<OdpowiedzWolneNegatywna> OdpowiedzWolneNegatywna { get; set; }
        public Event<Timeout> TimeoutEvent { get; private set; }
        public Schedule<Zamowienie, Timeout> TO { get; set; }

        public Sklep()
        {
            InstanceState(x => x.CurrentState);

            Event(() => StartZamowienia, x => x.CorrelateBy(s => s.Login, ctx => ctx.Message.Login).SelectId(context => Guid.NewGuid()));

            Schedule(() => TO, x => x.TimeoutId, x => x.Delay = TimeSpan.FromSeconds(10));

            Initially(
                When(StartZamowienia)
                    .Then(ctx =>
                    {
                        ctx.Instance.Login = ctx.Data.Login;
                        ctx.Instance.Ilosc = ctx.Data.Ilosc;
                        Console.WriteLine($"Klient {ctx.Data.Login} zamawia. Ilość: {ctx.Data.Ilosc}");
                    })
                    .Schedule(TO, ctx => new Timeout { CorrelationId = ctx.Instance.CorrelationId })
                    .Respond(ctx => new PytanieoPotwierdzenie { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login })
                    .Respond(ctx => new PytanieoWolne { CorrelationId = ctx.Instance.CorrelationId, Ilosc = ctx.Instance.Ilosc })
                    .TransitionTo(Niepotwierdzone)
            );

            During(Niepotwierdzone,
                When(TimeoutEvent)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"TIMEOUT: Klient {ctx.Instance.Login} na zamowienie {ctx.Data.CorrelationId}");
                    })
                    .Respond(ctx => new OdrzucenieZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize(),

                When(Potwierdzenie)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Klient: {ctx.Instance.Login} potwierdzil zamowienie {ctx.Data.CorrelationId}");
                    })
                    .Unschedule(TO)
                    .TransitionTo(PotwierdzoneKlient),

                When(BrakPotwierdzenia)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Klient: {ctx.Instance.Login} nie potwierdzil zamowienia {ctx.Data.CorrelationId}");
                    })
                    .Respond(ctx => new OdrzucenieZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize(),

                When(OdpowiedzWolne)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Zamówienie: {ctx.Data.CorrelationId} może być realizowane przez magazyn");
                    })
                    .TransitionTo(PotwierdzoneMagazyn),

                When(OdpowiedzWolneNegatywna)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Zamówienie: {ctx.Data.CorrelationId} nie może zostać zrealizowane przez magazyn");
                    })
                    .Respond(ctx => new OdrzucenieZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize()
            );

            During(PotwierdzoneKlient,
                When(OdpowiedzWolne)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Zamówienie: {ctx.Data.CorrelationId} może być realizowane przez magazyn");
                    })
                    .Respond(ctx => new AkceptacjaZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize(),

                When(OdpowiedzWolneNegatywna)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Zamówienie: {ctx.Data.CorrelationId} nie może zostać zrealizowane przez magazyn");
                    })
                    .Respond(ctx => new OdrzucenieZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize()
            );

            During(PotwierdzoneMagazyn,
                When(TimeoutEvent)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"TIMEOUT: Klient {ctx.Instance.Login} na zamowienie {ctx.Data.CorrelationId}");
                    })
                    .Respond(ctx => new OdrzucenieZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize(),

                When(Potwierdzenie)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Klient: {ctx.Instance.Login} potwierdzil zamowienie {ctx.Data.CorrelationId}");
                    })
                    .Respond(ctx => new AkceptacjaZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Unschedule(TO)
                    .Finalize(),

                When(BrakPotwierdzenia)
                    .Then(ctx =>
                    {
                        Console.WriteLine($"Klient: {ctx.Instance.Login} nie potwierdzil zamowienia {ctx.Data.CorrelationId}");
                    })
                    .Respond(ctx => new OdrzucenieZamowienia { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var repo = new InMemorySagaRepository<Zamowienie>();
            var saga = new Sklep();
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("amqps://auoyxxei:QE9BIbvPWsp4wwAS-OrTIKvupwppKGok@sparrow.rmq.cloudamqp.com/auoyxxei"), h =>
                {
                    h.Username("auoyxxei");
                    h.Password("QE9BIbvPWsp4wwAS-OrTIKvupwppKGok");
                });
                sbc.ReceiveEndpoint("saga", ep => ep.StateMachineSaga(saga, repo));
                sbc.UseInMemoryScheduler();
            });

            bus.Start();
            Console.WriteLine("Sklep");

            // Keep the application running
            Console.ReadLine();

            bus.Stop();
        }
    }
}