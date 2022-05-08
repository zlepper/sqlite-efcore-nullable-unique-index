using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EfCoreOverwriteIndexCreation.Tests;

[TestFixture]
public class SqlServerTests : BaseTest
{
    protected override void ConfigureContext(DbContextOptionsBuilder o)
    {
        o.UseSqlServer("Server=localhost;Database=MyDb;Trusted_Connection=False;user id=sa;password=Passw0rd;");
    }
}