﻿// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;

namespace OPZBot.Services.MessageBackup;

public interface IBackupService
{
    Task<TimeSpan> TimeFromLastBackupAsync(SocketInteractionContext interactionContext);
    Task DeleteUserAsync(SocketInteractionContext interactionContext);
}