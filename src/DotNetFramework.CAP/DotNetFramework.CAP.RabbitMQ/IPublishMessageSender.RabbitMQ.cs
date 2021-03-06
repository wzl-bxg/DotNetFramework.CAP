﻿// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using DotNetFramework.CAP.Internal;
using DotNetFramework.CAP.Processor.States;
using  DotNetFramework.CAP.Core.Logger;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace DotNetFramework.CAP.RabbitMQ
{
    internal sealed class RabbitMQPublishMessageSender : BasePublishMessageSender
    {
        private readonly IConnectionChannelPool _connectionChannelPool;
        private readonly ILogger _logger;
        private readonly string _exchange;

        public RabbitMQPublishMessageSender(ILogger<RabbitMQPublishMessageSender> logger, CapOptions options,
            IStorageConnection connection, IConnectionChannelPool connectionChannelPool, IStateChanger stateChanger)
            : base(logger, options, connection, stateChanger)
        {
            _logger = logger;
            _connectionChannelPool = connectionChannelPool;
            _exchange = _connectionChannelPool.Exchange;
            ServersAddress = _connectionChannelPool.HostAddress;
        }

        public override Task<OperateResult> PublishAsync(string keyName, string content)
        {
            var channel = _connectionChannelPool.Rent();
            try
            {
                var body = Encoding.UTF8.GetBytes(content);
                var props = new BasicProperties()
                {
                    DeliveryMode = 2
                };

                channel.ExchangeDeclare(_exchange, RabbitMQOptions.ExchangeType, true);
                channel.BasicPublish(_exchange, keyName, props, body);

                _logger.LogDebug($"RabbitMQ topic message [{keyName}] has been published. Body: {content}");

                return Task.FromResult(OperateResult.Success);
            }
            catch (Exception ex)
            {
                var wapperEx = new PublisherSentFailedException(ex.Message, ex);
                var errors = new OperateError
                {
                    Code = ex.HResult.ToString(),
                    Description = ex.Message
                };

                return Task.FromResult(OperateResult.Failed(wapperEx, errors));
            }
            finally
            {
                var returned = _connectionChannelPool.Return(channel);
                if (!returned)
                {
                    channel.Dispose();
                }
            }
        }
    }
}