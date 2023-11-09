﻿using App.Services.Database.Models;

namespace App.Services.Database.Repository
{
    internal static class ChannelRepository
    {
        public static Channel RegisterIfNotExists(Channel channel)
        {
            var _log = new ConsoleLogger(nameof(ChannelRepository));
            var context = DbConnection.GetConnection();
            var theChannel = context.Channels.SingleOrDefault(c => c.Id == channel.Id);

            if (theChannel is not null)
            {
                _log.BackupAction($"Channel '{channel.Name}' already has been added");
                return theChannel;
            }

            try
            {
                context.Channels.Add(channel);
                context.SaveChanges();
                _log.BackupAction($"Added new channel: '{channel.Name}'");
            }
            catch (Exception ex)
            {
                _log.Exception("Failed to save new channel entry", ex);
                throw;
            }
            return channel;
        }
    }
}