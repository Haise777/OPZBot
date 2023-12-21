﻿// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Microsoft.EntityFrameworkCore;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;
using OPZBot.Services.MessageBackup.FileBackup;

namespace OPZBot.Services.MessageBackup;

public class MessageProcessor : IBackupMessageProcessor
{
    private readonly IdCacheManager _cache;
    private readonly MyDbContext _dataContext;
    private readonly IFileBackupService _fileBackup;
    private readonly Mapper _mapper;

    public MessageProcessor(MyDbContext dataContext, IdCacheManager cache, Mapper mapper, IFileBackupService fileBackup)
    {
        _dataContext = dataContext;
        _cache = cache;
        _mapper = mapper;
        _fileBackup = fileBackup;
    }

    public event Action? EndBackupProcess;
    public bool IsUntilLastBackup { get; set; }

    public async Task<MessageDataBatchDto> ProcessMessagesAsync(IEnumerable<IMessage> messageBatch, uint registryId)
    {
        var existingMessageIds = await _dataContext.Messages
            .Where(x => x.ChannelId == messageBatch.First().Channel.Id)
            .Select(m => m.Id)
            .ToArrayAsync();

        var users = new List<User>();
        var messages = new List<Message>();
        var fileCount = 0;
        var concurrentDownloads = new List<Task>();

        foreach (var message in messageBatch)
        {
            if (message.Content == "" && message.Author.Id == Program.BotUserId) continue;
            if (existingMessageIds.Any(m => m == message.Id))
            {
                if (IsUntilLastBackup)
                {
                    EndBackupProcess?.Invoke();
                    break;
                }

                continue;
            }

            var mappedMessage = _mapper.Map(message, registryId);
            if (message.Attachments.Any())
            {
                concurrentDownloads.Add(_fileBackup.BackupFilesAsync(message));
                mappedMessage.File = @$"Backup\{message.Channel.Id}\{message.Id}";
                fileCount += message.Attachments.Count;
            }

            if (!await _cache.UserIds.ExistsAsync(message.Author.Id))
                users.Add(_mapper.Map(message.Author));
            messages.Add(mappedMessage);
        }

        await Task.WhenAll(concurrentDownloads);
        return new MessageDataBatchDto(users, messages, fileCount);
    }
}