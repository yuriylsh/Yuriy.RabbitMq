using LanguageExt;
using RabbitMQ.Client;
using Yuriy.RabbitMq.Options;

namespace Yuriy.RabbitMq
{
    public interface IRabbitMqConnectionService
    {
        Try<IConnection> GetConnection(EstablishConnectionOptions options);

        Try<IModel> CreateChannel(IConnection connection);

        Try<Unit> DeclareBindExchangeAndQueues(IModel channel, ExchangeOptions exchangeOptions, QueueOptions[] queueOptions);
    }
}