using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Resolves the appropriate service scope for SimpleTransit operations, 
/// preferring HTTP context scopes when available.
/// </summary>
internal sealed class SimpleTransitScopeResolver(IServiceProvider rootServiceProvider, IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Gets the appropriate service provider for the current context.
    /// If an HTTP request is in progress, returns the scoped service provider from the HTTP context.
    /// Otherwise, returns the root service provider.
    /// </summary>
    /// <returns>The appropriate service provider for the current context.</returns>
    public IServiceProvider GetCurrentServiceProvider()
    {
        // Check if there's an active HTTP context.
        if (httpContextAccessor.HttpContext is not null)
        {
            // Use the service provider from the current HTTP context.
            return httpContextAccessor.HttpContext.RequestServices;
        }

        // Fall back to the root service provider.
        return rootServiceProvider;
    }

    /// <summary>
    /// Creates a new asynchronous service scope.
    /// </summary>
    /// <remarks>The created <see cref="AsyncServiceScope"/> provides a scoped service provider  for resolving
    /// scoped services. It is the caller's responsibility to dispose  of the returned scope to release
    /// resources.</remarks>
    /// <returns>An <see cref="AsyncServiceScope"/> that represents the new service scope.</returns>
    public AsyncServiceScope CreateAsyncScope() =>
        rootServiceProvider.CreateAsyncScope();
}