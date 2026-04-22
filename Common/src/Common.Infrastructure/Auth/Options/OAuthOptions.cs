namespace Common.Infrastructure.Auth.Options;

public class OAuthOptions
{
    public string Domain { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string Audience { get; set; } = null!;
}
