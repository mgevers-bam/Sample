namespace Authentication.Infrastructure.Persistence.Options;

public class DatabaseOptions
{
    public string GolfAuthConnectionString { get; set; } = string.Empty;
    public string ConfigStoreConnectionString { get; set; } = string.Empty;
    public string OperationStoreConnectionString { get; set; } = string.Empty;
}
