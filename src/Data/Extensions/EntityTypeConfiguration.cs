using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Extensions
{
    public abstract class EntityTypeConfiguration<TEntity> where TEntity : class
    {
        public string Schema { get; private set; }

        public EntityTypeConfiguration(string schema)
        {
            this.Schema = schema;
        }

        public abstract void Map(EntityTypeBuilder<TEntity> builder);
    }
}
