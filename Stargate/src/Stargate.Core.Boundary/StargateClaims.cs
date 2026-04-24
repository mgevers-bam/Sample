using Authentication.Core.Boundary;

namespace Stargate.Core.Boundary;

public static class StargateClaims
{
    public static readonly ClaimRecord PersonRead = new ("person", "read");
    public static readonly ClaimRecord PersonWrite = new ("person", "write");

    public static readonly IReadOnlyCollection<ClaimRecord> Claims =
    [
        PersonRead,
        PersonWrite
    ];
}
