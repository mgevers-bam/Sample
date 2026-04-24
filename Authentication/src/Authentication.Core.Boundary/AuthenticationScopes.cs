namespace Authentication.Core.Boundary;

public static class AuthenticationScopes
{
    public static readonly IReadOnlyCollection<ScopeRecord> Scopes =
    [
        new ScopeRecord("openid", "OpenID", "Grants access to the OpenID Connect identity token"),
        new ScopeRecord("profile", "Profile", "Grants access to profile information (name, etc.)"),
        new ScopeRecord("email", "Email", "Grants access to email address"),
    ];
}
