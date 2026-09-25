using Microsoft.EntityFrameworkCore;

namespace PayNexa.SqlServer.Outbox;

public static class OutboxModelBuilderExtensions
{
    public const string TableName = "OutboxMessages";

    public static ModelBuilder ApplyOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable(TableName);
            outbox.HasKey(message => message.Id);
            outbox.Property(message => message.Id).ValueGeneratedNever();
            outbox.Property(message => message.Type).HasMaxLength(OutboxMessage.TypeMaxLength).IsRequired();
            outbox.Property(message => message.Payload).IsRequired();
            outbox.Property(message => message.CorrelationId).HasMaxLength(OutboxMessage.CorrelationIdMaxLength);
            outbox.Property(message => message.TraceParent).HasMaxLength(OutboxMessage.TraceParentMaxLength);
            outbox.Property(message => message.LastError).HasMaxLength(OutboxMessage.LastErrorMaxLength);
            outbox.HasIndex(message => new { message.NextAttemptAtUtc, message.OccurredAtUtc })
                .HasFilter($"[{nameof(OutboxMessage.ProcessedAtUtc)}] IS NULL")
                .HasDatabaseName("IX_OutboxMessages_Pending");
        });

        return modelBuilder;
    }
}
