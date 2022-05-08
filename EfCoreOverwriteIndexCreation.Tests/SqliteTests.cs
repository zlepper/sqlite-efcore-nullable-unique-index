using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
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

    protected override void ConfigureContext(DbContextOptionsBuilder o)
    {
        o.UseSqlite($"DataSource={basePath}/sqlite.db;");
        o.ReplaceService<IMigrationsSqlGenerator, MySqliteMigrationsSqlGenerator>();
    }
}