using ConsoleApp14;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Primitives;
using System;
using System.Threading.Tasks;
using static ConsoleApp1.Program;

namespace ConsoleApp1
{
    public class Program
    {
        public class FlightCancellation
        {
            public Guid FlightId { get; set; }

            public int CancellationId { get; set; }
        }
        public class FlightOrder
        {
            public Guid FlightId { get; set; }
            public int OrderId { get; set; }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var bus = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                cfg.Message<FlightOrder>(configTopology =>
                {
                    configTopology.SetEntityName("flight-orders");
                });

                var host = cfg.Host(new Uri("sb://platform361.servicebus.windows.net/"), x =>
                {
                    x.SharedAccessSignature(s =>
                    {
                        s.KeyName = "RootManageSharedAccessKey";
                        s.SharedAccessKey = "/1aH+hTp2gOgxiwfjd1l7v+y0jknRBKmKDu/upPaoPI=";
                        s.TokenTimeToLive = TimeSpan.FromDays(1);
                        s.TokenScope = TokenScope.Namespace;
                    });

                    x.TransportType = Microsoft.Azure.ServiceBus.TransportType.Amqp;
                });

                // setup Azure topic consumer
                cfg.SubscriptionEndpoint<FlightOrder>("flight-subscriber", configurator =>
                {
                    configurator.Consumer<FlightPurchasedConsumer>();
                });

                // setup Azure queue consumer
                cfg.ReceiveEndpoint("flight-cancellation", configurator =>
                {
                    configurator.Consumer<FlightCancellationConsumer>();
                });
            });

            bus.Start();

            Console.WriteLine("Started!");

            bus.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = new Random(1).Next(1, 999) });
            bus.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = new Random(1).Next(1, 999) });
            bus.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = new Random(1).Next(1, 999) });

            Console.WriteLine("Published!");

            Console.ReadLine();

            bus.Stop();
        }
    }

    public class FlightCancellationConsumer : IConsumer<FlightCancellation>
    {
        public Task Consume(ConsumeContext<FlightCancellation> context)
        {
            Console.WriteLine($"Flight Dequeued- FlightId:{context.Message.FlightId} CancellationId: {context.Message.CancellationId}");

            return Task.CompletedTask;
        }
    }

    public class FlightPurchasedConsumer : IConsumer<FlightOrder>
    {
        public Task Consume(ConsumeContext<FlightOrder> context)
        {
            Console.WriteLine($"Order processed: FlightId:{context.Message.FlightId} - OrderId:{context.Message.OrderId}");

            return Task.CompletedTask;
        }
    }

}
