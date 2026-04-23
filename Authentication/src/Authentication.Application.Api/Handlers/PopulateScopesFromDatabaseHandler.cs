using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Authentication.Application.Api.Handlers;

public class PopulateScopesFromDatabaseHandler : IOpenIddictServerHandler<HandleConfigurationRequestContext>
{
    private readonly IOpenIddictScopeManager _scopeManager;

    public PopulateScopesFromDatabaseHandler(IOpenIddictScopeManager scopeManager)
    {
        _scopeManager = scopeManager;
    }

    public static OpenIddictServerHandlerDescriptor Descriptor { get; } =
        OpenIddictServerHandlerDescriptor.CreateBuilder<HandleConfigurationRequestContext>()
            .UseScopedHandler<PopulateScopesFromDatabaseHandler>()
            .SetOrder(OpenIddictServerHandlers.Discovery.AttachScopes.Descriptor.Order + 1)
            .SetType(OpenIddictServerHandlerType.Custom)
            .Build();

    public async ValueTask HandleAsync(HandleConfigurationRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await foreach (var scope in _scopeManager.ListAsync())
        {
            var scopeName = await _scopeManager.GetNameAsync(scope);
            if (!string.IsNullOrEmpty(scopeName) && !context.Scopes.Contains(scopeName))
            {
                context.Scopes.Add(scopeName);
            }
        }
    }
}
