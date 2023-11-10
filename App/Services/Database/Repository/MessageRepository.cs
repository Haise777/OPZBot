﻿using App.Services.Database.Models;
using App.Utilities;

namespace App.Services.Database.Repository
{
    internal static class MessageRepository
    {
        private readonly static ConsoleLogger _log = new(nameof(MessageRepository));

        public static bool CheckIfExists(ulong id)
        {
            var context = DbConnection.GetConnection();
            return context.Messages.Any(m => m.Id == id);
        }

        public static void SaveToDatabase(List<Message> messagesToSave)
        {

            var context = DbConnection.GetConnection();

            try
            {
                context.Messages.AddRange(messagesToSave);
                context.SaveChanges();
                _log.BackupAction($"Saved {messagesToSave.Count} messages to database");
            }
            catch (Exception ex)
            {
                _log.Exception("Failed to save message batch to database", ex);
                throw;
            }
        }
    }
}
