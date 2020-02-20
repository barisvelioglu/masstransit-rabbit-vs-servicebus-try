using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.ServiceBus;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MassTransitExample.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ConsoleApp14
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(config =>
                    {
                        config.AddConsumer<FlightPurchasedConsumer>();
                        config.AddConsumer<FlightCancellationConsumer>();
                        config.AddBus(ConfigureBus);
                    });

                    services.AddSingleton<IHostedService, MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                });



            await builder.RunConsoleAsync();
        }

        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            string connectionString =
                "Endpoint=sb://platform361.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/1aH+hTp2gOgxiwfjd1l7v+y0jknRBKmKDu/upPaoPI=";

            string flightOrdersTopic = "flight-orders";

            string subscriptionName = "flight-subscriber";

            string queueName = "flight-cancellation";

            var rabbitBus = Bus.Factory.CreateUsingRabbitMq(busFactoryConfig =>
            {
                busFactoryConfig.Host("127.0.0.1", "/", host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });

                //eğer bunu koyarsam flight-orders diye fanout bir exchange oluşturuyor
                //busFactoryConfig.Message<FlightOrder>(m => { m.SetEntityName(flightOrdersTopic); });

                busFactoryConfig.ReceiveEndpoint(queueName, configurator =>
                {
                    configurator.ExchangeType = ExchangeType.Topic;
                    configurator.Consumer<FlightCancellationConsumer>(provider);
                });

                busFactoryConfig.ReceiveEndpoint(flightOrdersTopic, configurator =>
                {
                    configurator.ExchangeType = ExchangeType.Topic;
                    configurator.Consumer<FlightPurchasedConsumer>(provider);
                });
            });

            var azureServiceBus = Bus.Factory.CreateUsingAzureServiceBus(busFactoryConfig =>
            {
                //bu isimde bir topic oluşturuyor
                busFactoryConfig.Message<FlightOrder>(m => { m.SetEntityName(flightOrdersTopic); });

                var host = busFactoryConfig.Host(connectionString, hostConfig =>
                {
                    hostConfig.TransportType = TransportType.AmqpWebSockets;
                });

                // setup Azure topic consumer
                //flight-subscriber isimli bir subscribeını flight-orders topici için oluşturuyor
                busFactoryConfig.SubscriptionEndpoint<FlightOrder>(subscriptionName, configurator =>
                {
                    configurator.Consumer<FlightPurchasedConsumer>(provider);
                });

                // setup Azure queue consumer
                busFactoryConfig.ReceiveEndpoint(queueName, configurator =>
                {
                    configurator.Consumer<FlightCancellationConsumer>(provider);
                    configurator.SubscribeMessageTopics = false;
                    // as this is a queue, no need to subscribe to topics, so set this to false.

                });
            });

            return azureServiceBus;
        }
    }

}
