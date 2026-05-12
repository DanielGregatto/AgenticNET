using Data.Extensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Mappings
{
    public class ConversationMapping : EntityTypeConfiguration<Conversation>
    {
        public ConversationMapping(string schema) : base(schema) { }

        public override void Map(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(Schema + nameof(Conversation));

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(x => x.UserId);

            builder.Ignore(x => x.DomainEvents);
        }
    }
}
