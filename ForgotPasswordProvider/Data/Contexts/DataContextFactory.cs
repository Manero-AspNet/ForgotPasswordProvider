using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForgotPasswordProvider.Data.Contexts;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer(@"Server=tcp:nackademincms23.database.windows.net,1433;Initial Catalog=Nackademin-Db;Persist Security Info=False;User ID=NackademinAdmin;Password=CMS23Manero;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        return new DataContext(optionsBuilder.Options);
    }
}
