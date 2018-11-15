using System;
using System.Threading;
using RabbitMQ.Client;

namespace Yuriy.RabbitMq.Options
{
    public class EstablishConnectionOptions
    {
        public EstablishConnectionOptions(
            RabbitMqConnectionService.OnRetry onRetry,
            TimeSpan? connectionTimeout = null,
            bool? retry = true,
            ushort heartbeat = 12,
            CancellationToken? token = null)
        {
            Heartbeat = heartbeat;
            OnRetry = onRetry ?? NoopOnRetry;
            ConnectionTimeout = connectionTimeout.HasValue
                ? (int) connectionTimeout.Value.TotalMilliseconds
                : ConnectionFactory.DefaultConnectionTimeout;
            Retry = retry ?? true;
            Token = token ?? CancellationToken.None;
        }

        public RabbitMqConnectionService.OnRetry OnRetry { get; }

        public int ConnectionTimeout { get; }

        public bool Retry { get; }

        public ushort Heartbeat { get; }

        public CancellationToken Token { get; }

        private void NoopOnRetry(int _) {}
    }
}