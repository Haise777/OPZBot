﻿// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.RegularExpressions;
using Discord;
using Microsoft.Extensions.Logging;
using OPZBot.Logging;

namespace OPZBot.Services.MessageBackup.FileBackup;

public class FileBackupService : IFileBackupService
{
    private const int FileExtensionLimit = 8;
    private static readonly Regex MatchFileExtension = new(@"\.([^\.]+?)(?=\?ex)");
    private readonly HttpClient _client;
    private readonly SemaphoreSlim _downloadLimiter = new(50, 50);
    private readonly ILogger<FileBackupService> _logger;

    public FileBackupService(HttpClient client, ILogger<FileBackupService> logger)
    {
        _client = client;
        _logger = logger;
        if (!Directory.Exists(Program.FileBackupPath))
            Directory.CreateDirectory(Program.FileBackupPath);
    }

    public async Task BackupFilesAsync(IMessage message)
    {
        await _downloadLimiter.WaitAsync();
        try
        {
            if (!message.Attachments.Any()) return;
            await CreateChannelDirIfNotExists(message);

            if (message.Attachments.Count > 1)
            {
                await BackupMultipleFiles(message);
                return;
            }

            var fileUrl = message.Attachments.First().Url;
            var extension = GetExtension(message);
            // var extension = MatchFileExtension.Match(fileUrl).Value;
            // if (extension.Length > FileExtensionLimit) extension = "";

            var file = await DownloadFile(fileUrl);

            await File.WriteAllBytesAsync(
                $"{Program.FileBackupPath}/{message.Channel.Id}/{message.Id}{extension}", file);
        }
        finally
        {
            _downloadLimiter.Release();
        }
    }

    public string GetExtension(IMessage message)
    {
        if (message.Attachments.Count > 1) return "";

        var extension = MatchFileExtension.Match(message.Attachments.First().Url).Value;
        return extension.Length > FileExtensionLimit ? "" : extension;
    }

    private async Task BackupMultipleFiles(IMessage message)
    {
        var dirPath = $"{Program.FileBackupPath}/{message.Channel.Id}/{message.Id}";
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        var n = 0;
        foreach (var attachment in message.Attachments)
        {
            var file = await DownloadFile(attachment.Url);
            var extension = MatchFileExtension.Match(attachment.Url).Value;
            if (extension.Length > FileExtensionLimit) extension = "";

            await File.WriteAllBytesAsync($"{dirPath}/file{++n}{extension}", file);
        }
    }

    private async Task<byte[]> DownloadFile(string url)
    {
        var attempts = 0;
        while (true)
            try
            {
                var file = await _client.GetByteArrayAsync(url);
                return file;
            }
            catch (HttpRequestException ex)
            {
                if (++attempts > 3) throw;

                await _logger.RichLogErrorAsync(ex, "{service}: ", nameof(FileBackupService));
                await Task.Delay(5000);
            }
    }

    private Task CreateChannelDirIfNotExists(IMessage message)
    {
        return Task.Run(() =>
        {
            if (!Directory.Exists($"{Program.FileBackupPath}/{message.Channel.Id}"))
                Directory.CreateDirectory($"{Program.FileBackupPath}/{message.Channel.Id}");
        });
    }
}