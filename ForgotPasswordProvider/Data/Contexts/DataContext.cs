using ForgotPasswordProvider.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ForgotPasswordProvider.Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<UserEntitiy>(options)
{
    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
    public DbSet<ForgotPasswordRequestEntity> ForgotPasswordRequests { get; set; }
}
