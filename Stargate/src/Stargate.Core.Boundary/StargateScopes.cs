using Authentication.Core.Boundary;

namespace Stargate.Core.Boundary;

public static class StargateScopes
{
    private const string StargateApi = "stargate-api";
    private const string StargateApiResource = "stargate.api";

    public static readonly IReadOnlyCollection<ScopeRecord> Scopes =
    [
        new (
            name: StargateApi,
            displayName: "Stargate API Access",
            description: "Allows access to the Stargate API",
            resources: [StargateApiResource],
            claims: StargateClaims.Claims)
    ];
}
