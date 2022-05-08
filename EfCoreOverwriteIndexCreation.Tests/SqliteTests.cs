using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EfCoreOverwriteIndexCreation.Tests;

[TestFixture]
public class SqliteTests : BaseTest
{

    const string basePath = "./testsqlite";

    [OneTimeSetUp]
    public void Setup()
    {
        Directory.CreateDirectory(basePath);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        // Directory.Delete(basePath, true);
    }
    
    protected override void ConfigureContext(DbContextOptionsBuilder o)
    {
        o.UseSqlite($"DataSource={basePath}/sqlite.db;");
    }
}