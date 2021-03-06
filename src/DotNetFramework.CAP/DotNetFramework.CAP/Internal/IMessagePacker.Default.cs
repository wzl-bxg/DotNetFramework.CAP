﻿// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using DotNetFramework.CAP.Abstractions;
using DotNetFramework.CAP.Models;

namespace DotNetFramework.CAP.Internal
{
    internal class DefaultMessagePacker : IMessagePacker
    {
        private readonly IContentSerializer _serializer;

        public DefaultMessagePacker(IContentSerializer serializer)
        {
            _serializer = serializer;
        }

        public string Pack(CapMessage obj)
        {
            return _serializer.Serialize(obj);
        }

        public CapMessage UnPack(string packingMessage)
        {
            return _serializer.DeSerialize<CapMessageDto>(packingMessage);
        }
    }
}