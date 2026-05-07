using Data.Context;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unit.Tests.Helpers;

public static class MockDbContextHelper
{
    public static Mock<AppDbContext> CreateMockDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockContext = new Mock<AppDbContext>(options);
        return mockContext;
    }

    public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryableData = data.AsQueryable();

        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

        return mockSet;
    }

    public static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["Schema"]).Returns("dbo");
        mockConfiguration.Setup(c => c["DefaultSchema"]).Returns("dbo");

        return new AppDbContext(options, mockConfiguration.Object);
    }

    public static async Task<AppDbContext> SeedProductData(this AppDbContext context, List<Product> products)
    {
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
        return context;
    }
}
