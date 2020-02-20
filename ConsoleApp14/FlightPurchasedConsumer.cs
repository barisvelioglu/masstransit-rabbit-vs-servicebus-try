﻿using MassTransit;
using System.Threading.Tasks;
using MassTransitExample.Messages;
using Microsoft.Extensions.Logging;

namespace ConsoleApp14
{
    public class FlightPurchasedConsumer : IConsumer<FlightOrder>
    {
        private readonly ILogger<FlightPurchasedConsumer> _logger;

        public FlightPurchasedConsumer(ILogger<FlightPurchasedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<FlightOrder> context)
        {

            _logger.LogInformation($"Order processed: FlightId:{context.Message.FlightId} - OrderId:{context.Message.OrderId}");

            return Task.CompletedTask;
        }
    }

}
