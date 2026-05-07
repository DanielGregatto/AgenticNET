using Data.Events;
using Data.Events.Interfaces;
using Data.Extensions;
using Data.Mappings;
using Domain;
using Domain.Core.Events.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Context
{
    public class AppDbContext : DbContext
    {
        private readonly string _schema;
        private readonly string _defaultSchema;
        private readonly IConfiguration _configuration;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _schema = _configuration["Schema"];
            _defaultSchema = _configuration["DefaultSchema"];
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_defaultSchema);
            modelBuilder.AddConfiguration(new ProductMapping(this._schema));
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conection = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(conection))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                optionsBuilder.UseSqlServer(conection);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                                       .Where(e => e.State is EntityState.Added
                                                           or EntityState.Modified 
                                                           or EntityState.Deleted)
                                       .ToList();

            foreach (var entry in entries)
            {
                if (entry.Entity is not IHasDomainEvents hasEvents)
                    continue;

                var entity = entry.Entity;
                var entityType = entity.GetType();

                object notification = entry.State switch
                {
                    EntityState.Added =>
                        CreateInfraEvent(typeof(EntityInsertedEvent<>), entityType, entity),

                    EntityState.Modified =>
                        CreateInfraEvent(typeof(EntityUpdatedEvent<>), entityType, entity),

                    EntityState.Deleted =>
                        CreateInfraEvent(typeof(EntityDeletedEvent<>), entityType, entity),

                    _ => null
                };

                if (notification != null)
                    hasEvents.RaiseDomainEvent(notification);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


        /// <summary>
        /// Gets all domain events from tracked entities
        /// </summary>
        public IEnumerable<object> GetDomainEvents()
        {
            return ChangeTracker.Entries()
                .Where(e => e.Entity is IHasDomainEvents)
                .Select(e => e.Entity as IHasDomainEvents)
                .Where(e => e.DomainEvents.Any())
                .SelectMany(e => e.DomainEvents)
                .ToList();
        }

        /// <summary>
        /// Clears domain events from all tracked entities
        /// </summary>
        public void ClearDomainEvents()
        {
            ChangeTracker.Entries()
                .Where(e => e.Entity is IHasDomainEvents)
                .Select(e => e.Entity as IHasDomainEvents)
                .Where(e => e.DomainEvents.Any())
                .ToList()
                .ForEach(e => e.ClearDomainEvents());
        }

        private static object CreateInfraEvent(Type openGenericEventType, Type entityType, object entity)
        {
            var closedType = openGenericEventType.MakeGenericType(entityType);
            return Activator.CreateInstance(closedType, entity)!;
        }
    }
}