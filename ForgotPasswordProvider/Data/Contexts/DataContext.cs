using ForgotPasswordProvider.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ForgotPasswordProvider.Data.Contexts;

public class DataContext : IdentityDbContext<UserEntitiy>
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
    public DbSet<ForgotPasswordRequestEntity> ForgotPasswordRequests { get; set; }
}
