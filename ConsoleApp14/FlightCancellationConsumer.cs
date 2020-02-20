using MassTransit;
using System.Threading.Tasks;
using MassTransitExample.Messages;
using Microsoft.Extensions.Logging;

namespace ConsoleApp14
{
    public class FlightCancellationConsumer : IConsumer<FlightCancellation>
    {
        private readonly ILogger<FlightCancellationConsumer> _logger;

        public FlightCancellationConsumer(ILogger<FlightCancellationConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<FlightCancellation> context)
        {
            _logger.LogInformation($"Flight Dequeued- FlightId:{context.Message.FlightId} CancellationId: {context.Message.CancellationId}");

            return Task.CompletedTask;
        }
    }

}
