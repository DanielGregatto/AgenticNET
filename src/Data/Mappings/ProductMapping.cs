using Data.Extensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Mappings
{
    public class ProductMapping : EntityTypeConfiguration<Product>
    {
        public ProductMapping(string schema) : base(schema)
        {
        }

        public override void Map(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(Schema + nameof(Product));

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Property(x => x.Sku)
                .HasMaxLength(1000);

            builder.Property(x => x.ImageFileName)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            builder.HasIndex(x => x.Sku);
        }
    }
}
