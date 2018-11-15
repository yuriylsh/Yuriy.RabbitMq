using System;
using System.Collections.Generic;
using LanguageExt;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using Yuriy.RabbitMq.Options;

namespace Yuriy.RabbitMq
{
    public class RabbitMqConnectionService: IRabbitMqConnectionService
    {
        public delegate void OnRetry(int retryCounter);

        private const string OnRetryContextKey = "onRetry";
        private Result<IConnection> _connection;
        private static readonly TimeSpan RetrySleepDuration = TimeSpan.FromSeconds(5);
        private static readonly RetryPolicy<IConnection> RetryConnectionPolicy;
        private readonly IRabbitMqConnectionConfiguration _config;
        private readonly object _lock = new object();

        private static Result<T> ToResult<T>(T value) => new Result<T>(value);

        private static readonly Result<Unit> DefaultUnitResult = ToResult(Unit.Default);

        static RabbitMqConnectionService()
        {
            RetryConnectionPolicy = Policy
                .Handle<Exception>()
                .OrResult<IConnection>(x => x.IsOpen != true)
                .WaitAndRetryForever((counter, context) =>
            {
                if(context.TryGetValue(OnRetryContextKey, out var onRetryHandler) && onRetryHandler is OnRetry onRetry )
                {
                    onRetry(counter);
                }

                return RetrySleepDuration;
            });
        }

        public RabbitMqConnectionService(IRabbitMqConnectionConfiguration config) => _config = config;

        public Try<IConnection> GetConnection(EstablishConnectionOptions options) => () =>
        {
            if(_connection == default)
            {
                lock (_lock)
                {
                    if(_connection == default) _connection = EstablishNewConnection(options);
                }
            }

            return _connection;
        };

        private Result<IConnection> EstablishNewConnection(EstablishConnectionOptions options)
        {
            var factory = new ConnectionFactory
            {
                RequestedHeartbeat = options.Heartbeat,
                AutomaticRecoveryEnabled = true,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                HostName = _config.HostName,
                RequestedConnectionTimeout = options.ConnectionTimeout
            };

            var retryContext = new Context(nameof(RabbitMqConnectionService), new Dictionary<string, object>{[OnRetryContextKey] = options.OnRetry});
            var connection = options.Retry
                ? RetryConnectionPolicy.Execute((_, __) => factory.CreateConnection(), retryContext, options.Token)
                : factory.CreateConnection();
            return ToResult(connection);
        }

        public Try<IModel> CreateChannel(IConnection connection) => () => ToResult(connection.CreateModel());

        public Try<Unit> DeclareBindExchangeAndQueues(IModel channel, ExchangeOptions exchangeOptions, QueueOptions[] queueOptions) => () =>
        {
            channel.ExchangeDeclare(exchangeOptions.Name, exchangeOptions.Type, exchangeOptions.IsDurable, exchangeOptions.IsAutoDelete);
            foreach (var queue in queueOptions ?? Array.Empty<QueueOptions>())
            {
                channel.QueueDeclare(queue.Name, queue.IsDurable, queue.IsExclusive, queue.IsAutoDelete);
                channel.QueueBind(queue.Name, exchangeOptions.Name, queue.RoutingKey);
            }
            return DefaultUnitResult;
        };
    }
}