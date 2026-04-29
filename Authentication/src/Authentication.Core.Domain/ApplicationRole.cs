using Microsoft.AspNetCore.Identity;

namespace Authentication.Core.Domain;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
