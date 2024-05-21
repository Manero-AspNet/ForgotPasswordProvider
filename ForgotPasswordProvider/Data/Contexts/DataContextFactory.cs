using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForgotPasswordProvider.Data.Contexts;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer(@"Server=tcp:cms23manerosqlserver.database.windows.net,1433;Initial Catalog=Manero-SqlDb;Persist Security Info=False;User ID=SqlAdmin;Password=CMS23Manero;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        return new DataContext(optionsBuilder.Options);
    }
}
