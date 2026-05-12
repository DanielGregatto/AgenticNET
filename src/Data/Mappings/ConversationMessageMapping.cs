using Data.Extensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Mappings
{
    public class ConversationMessageMapping : EntityTypeConfiguration<ConversationMessage>
    {
        public ConversationMessageMapping(string schema) : base(schema) { }

        public override void Map(EntityTypeBuilder<ConversationMessage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(Schema + nameof(ConversationMessage));

            builder.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Ignore(x => x.DomainEvents);
        }
    }
}
