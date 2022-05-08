using Microsoft.EntityFrameworkCore;

namespace EfCoreOverwriteIndexCreation;

public class MyContext : DbContext
{
    public MyContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TreeNode> TreeNodes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TreeNode>(treeNode =>
        {
            treeNode.HasKey(tn => tn.Id);
            
            treeNode.HasIndex(tn => new {tn.Name, tn.ParentId}).IsUnique().HasFilter(null);

            treeNode.HasMany(tn => tn.Children)
                .WithOne(c => c.Parent)
                .HasForeignKey(t => t.ParentId)
                .HasPrincipalKey(t => t.Id)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}

public class TreeNode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    
    public TreeNode? Parent { get; set; }

    public ICollection<TreeNode> Children { get; set; } = null!;
}