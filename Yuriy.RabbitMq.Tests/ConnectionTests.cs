using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using LanguageExt.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Xunit;
using Yuriy.RabbitMq.Options;

namespace Yuriy.RabbitMq.Tests
{
    public class ConnectionTests
    {
        private readonly EstablishConnectionOptions _noRetryOptions;

        public ConnectionTests()
        {
            _noRetryOptions = new EstablishConnectionOptions(ConnectionTestHelper.ConnectionLogError, TimeSpan.FromMilliseconds(100), false);
        }

        [Fact]
        public void GetConnection_ValidConfiguration_Connects()
        {
            var subject = new RabbitMqConnectionService(ConnectionTestHelper.ValidConfiguration);

            var result = subject.GetConnection(_noRetryOptions);

            result.ShouldBeSuccess(x =>
            {
                x.IsOpen.Should().BeTrue();
                using (var model = x.CreateModel()) model.ExchangeDeclarePassive("amq.direct");
            });
            ConnectionTestHelper.CloseConnection(result);
        }

        [Fact]
        public void GetConnection_PreviousConnectionStillOpen_ReturnsPreviousConnection()
        {
            var subject = new RabbitMqConnectionService(ConnectionTestHelper.ValidConfiguration);

            var consecutiveConnections = subject.GetConnection(_noRetryOptions)
                .Bind(first => subject.GetConnection(_noRetryOptions).Map(second => (first, second)));
            ConnectionTestHelper.CloseConnections(consecutiveConnections);

            consecutiveConnections.ShouldBeSuccess(connections => ReferenceEquals(connections.first, connections.second).Should().BeTrue());
        }

        [Theory]
        [InlineData("wrongUserName", "guest", "127.0.0.1", "test")]
        [InlineData("guest", "wrongpassword", "127.0.0.1", "test")]
        [InlineData("guest", "guest", "wronghost", "test")]
        [InlineData("guest", "guest", "127.0.0.1", "wrongvirtualhost")]
        public void GetConnection_InvalidConfiguration_Fails(string userName, string password, string hostName, string virtualHost)
        {
            var invalidConfig = new RabbitMqConnectionConfiguration
            {
                UserName = userName,
                Password = password,
                HostName = hostName,
                VirtualHost = virtualHost
            };
            var subject = new RabbitMqConnectionService(invalidConfig);

            var result = subject.GetConnection(_noRetryOptions);

            result.ShouldBeFail(ex => ex.Should().BeOfType<BrokerUnreachableException>());
        }

        [Fact]
        public void GetConnection_Fails_Retries()
        {
            var config = ConnectionTestHelper.ValidConfiguration;
            config.Password = "not going to work";
            int numberOfRetries = 0;
            var subject = new RabbitMqConnectionService(config);
            var cancelSource = new CancellationTokenSource();
            const int connectionTimeout = 5;

            var connection = subject.GetConnection(new EstablishConnectionOptions(OnRetry, TimeSpan.FromMilliseconds(connectionTimeout), true, token: cancelSource.Token));

            cancelSource.CancelAfter(connectionTimeout * 10);
            connection.ShouldBeFail(ex => ex.Should().BeOfType<OperationCanceledException>());

            numberOfRetries.Should().BeGreaterThan(0);

            void OnRetry(int retryCounter) => numberOfRetries = retryCounter;
        }

        [Fact]
        public void DeclareBindExchangeAndQueues_GivenExchangeAndQueues_DeclaresAndBindsExchangesAndQueues()
        {
            var exchangeOptions = new ExchangeOptions("test", ExchangeType.Topic, Durability.Transient, AutoDeletion.Yes);
            var queueOne = new QueueOptions("Queue One", "queue.one", Durability.Transient, AutoDeletion.No, Exclusivity.No);
            var queueTwo = new QueueOptions("Queue two", "queue.two", Durability.Durable, AutoDeletion.Yes, Exclusivity.Yes);
            var queues = new[] { queueOne, queueTwo };
            var subject = new RabbitMqConnectionService(ConnectionTestHelper.ValidConfiguration);

            subject.TestWithChannel(_noRetryOptions, (exchangeOptions.Name, queues.Select(x => x.Name)), channel =>
            {
                subject.DeclareBindExchangeAndQueues(channel, exchangeOptions, queues)
                    .ShouldBeSuccess(_ =>
                    {
                        channel.ExchangeDeclarePassive(exchangeOptions.Name);
                        channel.QueueDeclarePassive(queueOne.Name).QueueName.Should().Be(queueOne.Name);
                        channel.QueueDeclarePassive(queueTwo.Name).QueueName.Should().Be(queueTwo.Name);
                    });
            });
        }
    }
}