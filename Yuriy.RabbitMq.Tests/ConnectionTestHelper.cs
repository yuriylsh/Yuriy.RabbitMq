using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.UnitTesting;
using RabbitMQ.Client;
using Yuriy.RabbitMq.Options;

namespace Yuriy.RabbitMq.Tests
{
    public static class ConnectionTestHelper
    {
        public static RabbitMqConnectionConfiguration ValidConfiguration => new RabbitMqConnectionConfiguration
        {
            UserName = "guest",
            Password = "guest",
            HostName = "127.0.0.1",
            VirtualHost = "test"
        };

        public static void CloseConnection(Try<IConnection> tryConnection)
            => tryConnection.Map(connection => {connection.Close(); return Unit.Default;});

        public static void CloseConnections(Try<(IConnection first, IConnection second)> tryMultiple)
            => tryMultiple.Map(c => {c.first.Close(); c.second.Close(); return Unit.Default;});

        public static void ConnectionLogError(int retryCounter)
            => Console.WriteLine($"{nameof(ConnectionLogError)}: {retryCounter:0}");

        public static Unit Cleanup( this (IConnection connection, IModel channel) connectionWithChannel, string exchangeName, IEnumerable<string> queueNames)
        {
            try
            {
                connectionWithChannel.channel.ExchangeDelete(exchangeName);
                foreach (var queue in queueNames)
                {
                    connectionWithChannel.channel.QueueDelete(queue);
                }
                connectionWithChannel.connection.Close();
            }
            catch{}
            return Unit.Default;
        }

        

        public static Unit TestWithChannel(this RabbitMqConnectionService connectionService
            , EstablishConnectionOptions connectionOptions
            , (string exchangeName, IEnumerable<string> queueNames) cleanupParams
            , Action<IModel> validate)
        {
            connectionService.GetConnection(connectionOptions)
                .Bind(connection => connectionService.CreateChannel(connection).Map(channel => (connection, channel)))
                .ShouldBeSuccess(x =>
                {
                    try
                    {
                        validate(x.channel);
                    }
                    catch (Exception) when (DoCleanup()){ }
                    bool DoCleanup(){x.Cleanup(cleanupParams.exchangeName, cleanupParams.queueNames); return false;}
                });
            return Unit.Default;
        }
    }
}