using Data.Extensions;
using Domain;
using Domain.Contracts.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Text.Json;

namespace Data.Mappings
{
    public class ConversationTurnMapping : EntityTypeConfiguration<ConversationTurn>
    {
        public ConversationTurnMapping(string schema) : base(schema) { }

        public override void Map(EntityTypeBuilder<ConversationTurn> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(Schema + nameof(ConversationTurn));

            builder.Property(x => x.AgentName)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.UserMessage)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.AssistantMessage)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.TraceSteps)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<TraceStep>>(v, (JsonSerializerOptions)null) ?? new List<TraceStep>())
                .HasColumnType("nvarchar(max)");

            builder.Ignore(x => x.DomainEvents);
        }
    }
}
