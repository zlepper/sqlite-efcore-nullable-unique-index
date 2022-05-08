using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace EfCoreOverwriteIndexCreation.Tests;

public abstract class BaseTest
{

    protected abstract void ConfigureContext(DbContextOptionsBuilder o);
    
    private ServiceProvider CreateServiceProvider()
    {
        
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging(l => l.AddConsole());
        serviceCollection.AddDbContext<MyContext>(c =>
        {
            ConfigureContext(c);
            c.EnableSensitiveDataLogging();
        });

        return serviceCollection.BuildServiceProvider();
        

    }

    private async Task<(TreeNode Root, TreeNode Child)> CreateBasicTree(ServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var ctx = scope.ServiceProvider.GetRequiredService<MyContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BaseTest>>();
        logger.LogInformation("Deleted existing data: {@DeletedData}", await ctx.Database.EnsureDeletedAsync());
        logger.LogInformation("Created new data: {@CreatedData}", await ctx.Database.EnsureCreatedAsync());


        var root = new TreeNode()
        {
            Name = "Root"
        };
        ctx.TreeNodes.Add(root);

        var child = new TreeNode()
        {
            Name = "Child",
            Parent = root
        };
        
        ctx.TreeNodes.Add(child);
        await ctx.SaveChangesAsync();

        return (root, child);
    }

    [Test]
    public async Task ErrorsOnConflictingChild()
    {
        await using var serviceProvider = CreateServiceProvider();

        var (root, child) = await CreateBasicTree(serviceProvider);
        
        await using var scope = serviceProvider.CreateAsyncScope();

        var ctx = scope.ServiceProvider.GetRequiredService<MyContext>();
        var conflictingChild = new TreeNode()
        {
            Name = child.Name,
            ParentId = root.Id
        };
        
        ctx.TreeNodes.Add(conflictingChild);
        try
        {
            await ctx.SaveChangesAsync();
            Assert.Fail("No exception was thrown");
        }
        catch (DbUpdateException e) when (e.IsUniqueConstraintViolation())
        {
            // expected
        }
    }
    
    [Test]
    public async Task ErrorsOnConflictingRoot()
    {
        await using var serviceProvider = CreateServiceProvider();

        var (root, _) = await CreateBasicTree(serviceProvider);
        
        await using var scope = serviceProvider.CreateAsyncScope();

        var ctx = scope.ServiceProvider.GetRequiredService<MyContext>();
        
        var conflictingRoot = new TreeNode()
        {
            Name = root.Name,
            ParentId = null
        };
        
        ctx.TreeNodes.Add(conflictingRoot);
        try
        {
            await ctx.SaveChangesAsync();
            Assert.Fail("No exception was thrown");
        }
        catch (DbUpdateException e) when (e.IsUniqueConstraintViolation())
        {
            // expected
        }
    }
}