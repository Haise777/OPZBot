﻿using Discord;

namespace OPZBot.Services.MessageBackup;

public class BackupResponseBuilder
{
    //TODO Fix DateTime using wrong timezones
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public IMessage? StartMessage { get; set; }
    public IMessage? LastMessage { get; set; }
    public IUser? Author { get; set; }

    public Embed Build(int batchNumber, int numberOfMessages, BackupStage stage)
    {
        var embedBuilder = ConstructEmbed();
        var t = (DateTime.Now - StartTime)!.Value;
        var elapsed = $"{t.TotalHours:00}:{t:mm\\:ss}";

        switch (stage)
        {
            case BackupStage.Started:
                embedBuilder
                    .WithTitle("Em progresso...")
                    .WithColor(Color.Gold)
                    .AddField("progresso:",
                        $"Decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"Ciclos realizados: {batchNumber}\n" +
                        "Atual: ...");
                break;

            case BackupStage.InProgress:
                embedBuilder
                    .WithTitle("Em progresso...")
                    .WithColor(Color.Gold)
                    .AddField("Progresso:",
                        $"Decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"Ciclos realizados: {batchNumber}\n" +
                        $"Atual: {LastMessage.Author} {LastMessage.Timestamp.DateTime.ToShortDateString()} {LastMessage.Timestamp.DateTime.ToShortTimeString()}" +
                        $"\n{LastMessage.Content}");
                break;

            case BackupStage.Finished:
                embedBuilder
                    .WithTitle("Backup finalizado")
                    .WithColor(Color.Green)
                    .AddField("Estatisticas:",
                        $"Tempo decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"Ciclos realizados: {batchNumber}");
                break;

            case BackupStage.Failed:
                embedBuilder
                    .WithTitle("Falhou") //TODO rework failed response
                    .WithColor(Color.Red)
                    .AddField("Estatisticas:",
                        $"Tempo decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"Ciclos realizados: {batchNumber}");
                break;
        }

        return embedBuilder.Build();
    }

    private string[] ParseValuesToStrings()
    {
        var parsedValues = new string[4];
//TODO All this ternary operation really the best way?
        parsedValues[0] = StartMessage is not null
            ? $"{StartMessage.Author.Username} {StartMessage.Timestamp.DateTime.ToShortDateString()} {StartMessage.Timestamp.DateTime.ToShortTimeString()}" +
              $"\n{StartMessage.Content}"
            : "...";
        parsedValues[1] = LastMessage is not null
            ? $"{LastMessage.Author.Username} {LastMessage.Timestamp.DateTime.ToShortDateString()} {LastMessage.Timestamp.DateTime.ToShortTimeString()}" +
              $"\n{LastMessage.Content}"
            : "...";
        parsedValues[2] = StartTime.HasValue
            ? StartTime.Value.ToLongTimeString()
            : throw new InvalidOperationException("StartTime builder property is not optional");
        parsedValues[3] = EndTime.HasValue
            ? EndTime.Value.ToLongTimeString()
            : "...";

        return parsedValues;
    }

    private EmbedBuilder ConstructEmbed()
    {
        if (Author is null) throw new InvalidOperationException("Author property was not set");
        var values = ParseValuesToStrings();

        var firstMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("De:")
            .WithValue(values[0])
            .WithIsInline(false);
        var lastMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("Até:")
            .WithValue(values[1])
            .WithIsInline(false);

        var startTimeEmbed = new EmbedFieldBuilder()
            .WithName("Iniciado:")
            .WithValue(values[2])
            .WithIsInline(true);
        var endTimeEmbed = new EmbedFieldBuilder()
            .WithName("Terminado:")
            .WithValue(values[3])
            .WithIsInline(true);

        var madeByEmbed = new EmbedFooterBuilder()
            .WithText($"por: {Author.Username}")
            .WithIconUrl(Author.GetAvatarUrl());

        var embedBuilder = new EmbedBuilder()
            .AddField(firstMessageFieldEmbed)
            .AddField(lastMessageFieldEmbed)
            .AddField(startTimeEmbed)
            .AddField(endTimeEmbed)
            .WithFooter(madeByEmbed);

        return embedBuilder;
    }
}

public enum BackupStage //TODO Should this really be here?
{
    Started,
    InProgress,
    Finished,
    Failed
}