using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EfCoreOverwriteIndexCreation.Tests;

public static class Helpers
{
    
    public static bool IsUniqueConstraintViolation(this DbUpdateException e)
    {
        // From https://stackoverflow.com/a/47465944/3950006
        SqlException sqlEx;
        if (e.InnerException is SqlException ex)
        {
            sqlEx = ex;
        }
        else if (e.InnerException?.InnerException is SqlException sex)
        {
            sqlEx = sex;
        }
        else
        {
            return e.InnerException is SqliteException{ SqliteErrorCode: 19, SqliteExtendedErrorCode: 1555 or 2067};
        }

        return sqlEx.Number is 2601 or 2627;
    }
}