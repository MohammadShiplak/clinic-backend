using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Common;

public static class DbUpdateExceptionExtensions
{
    // 2601 = duplicate key in a unique index, 2627 = unique/primary key constraint violation
    public static bool IsUniqueViolation(this DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}