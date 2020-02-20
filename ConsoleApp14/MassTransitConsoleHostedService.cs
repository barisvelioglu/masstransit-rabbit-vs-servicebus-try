using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using MassTransitExample.Messages;
using System;

namespace ConsoleApp14
{
    public class MassTransitConsoleHostedService : IHostedService
    {
        readonly IBusControl _bus;

        public MassTransitConsoleHostedService(IBusControl bus, ILoggerFactory loggerFactory)
        {
            _bus = bus;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _bus.StartAsync(cancellationToken).ConfigureAwait(false);

            var _random = new Random();

            await _bus.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = _random.Next(1, 999) });
            await _bus.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = _random.Next(1, 999) });
            await _bus.Publish<FlightOrder>(new FlightOrder { FlightId = Guid.NewGuid(), OrderId = _random.Next(1, 999) });

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _bus.StopAsync(cancellationToken);
        }
    }

}
