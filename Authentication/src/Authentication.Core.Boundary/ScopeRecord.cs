namespace Authentication.Core.Boundary;

public class ScopeRecord(
    string name,
    string displayName,
    string description,
    HashSet<string>? resources = null,
    IReadOnlyCollection<ClaimRecord>? claims = null)
{
    public string Name { get; private set; } = name;
    public string DisplayName { get; private set; } = displayName;
    public string Description { get; private set; } = description;
    public HashSet<string> Resources { get; private set; } = resources ?? [];
    public IReadOnlyCollection<ClaimRecord> Claims { get; private set; } = claims ?? [];
}
