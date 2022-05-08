using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EfCoreOverwriteIndexCreation.Tests;

public class MySqliteMigrationsSqlGenerator : SqliteMigrationsSqlGenerator
{

    public MySqliteMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
    {
    }
    
    protected override void Generate(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        ArgumentNullException.ThrowIfNull(model);
        
        if (!operation.IsUnique)
        {
            base.Generate(operation, model, builder, terminate);
            return;
        }



        var entityType = model.GetEntityTypes().FirstOrDefault(e => e.GetTableName() == operation.Table);
        
        if (entityType == null)
        {
            base.Generate(operation, model, builder, terminate);
            return;
        }

        var updatedColumns = operation.Columns.ToArray();
        var changedColumnIndices = new bool[updatedColumns.Length];

        for (var index = 0; index < updatedColumns.Length; index++)
        {
            var column = updatedColumns[index];
            var property = entityType.GetProperties().FirstOrDefault(p =>
                p.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema())) == column);

            if (property == null)
            {
                base.Generate(operation, model, builder, terminate);
                return;
            }

            if (property.IsNullable)
            {
                var ownName = Dependencies.SqlGenerationHelper.DelimitIdentifier(column);
                updatedColumns[index] = $"ifnull({ownName}, 0)";
                changedColumnIndices[index] = true;
            }
        }

        if (!changedColumnIndices.Any(c => c))
        {
            base.Generate(operation, model, builder, terminate);
            return;
        }


        operation.Columns = updatedColumns;
        
        builder.Append("CREATE UNIQUE ");
        
        IndexTraits(operation, model, builder);
        
        builder
            .Append("INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" ON ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" (");
            // .Append(ColumnList(operation.Columns))
            
            for (var i = 0; i < operation.Columns.Length; i++)
            {
                var column = operation.Columns[i];
                if(i > 0)
                    builder.Append(", ");
                
                if (changedColumnIndices[i])
                {
                    builder.Append(column);
                }
                else
                {
                    builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(column));
                }
            }
            
            builder
            .Append(")");

        IndexOptions(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

}