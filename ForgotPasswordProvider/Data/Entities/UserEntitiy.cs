using Microsoft.AspNetCore.Identity;

namespace ForgotPasswordProvider.Data.Entities;

public class UserEntitiy : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool Verified { get; set; }
}
