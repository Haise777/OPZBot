﻿// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.WebSocket;

namespace OPZBot.Services;

public interface IMessageFetcher
{
    Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel);
    Task<IEnumerable<IMessage>> FetchAsync(ISocketMessageChannel channel, ulong startFrom);
}