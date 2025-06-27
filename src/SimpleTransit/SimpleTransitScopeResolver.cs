using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleTransit;

/// <summary>
/// Resolves the appropriate service scope for SimpleTransit operations, 
/// preferring HTTP context scopes when available.
/// </summary>
/// <remarks>
/// <para>
/// This resolver ensures that service resolution occurs within the correct scope, which is critical
/// for proper lifetime management of scoped services in ASP.NET Core applications. It automatically
/// detects whether an HTTP request context is available and uses the appropriate service provider.
/// </para>
/// <para>
/// This component is essential for maintaining proper dependency injection scoping across different
/// execution contexts, such as HTTP requests and background operations.
/// </para>
/// </remarks>
internal sealed class SimpleTransitScopeResolver(IServiceProvider rootServiceProvider, IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Retrieves an <see cref="IServiceProvider"/> for resolving dependencies, preferring the current HTTP request's scope if available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is designed to ensure that service resolution occurs within the correct scope, which is critical for the correct
    /// lifetime management of scoped services in ASP.NET Core applications. If an HTTP request is active, it uses the request's
    /// <see cref="IServiceProvider"/> to ensure that all resolved services are tied to the request's lifetime. This prevents issues
    /// such as leaking scoped services across requests or resolving services outside of their intended scope.
    /// </para>
    /// <para>
    /// If no HTTP context is available (e.g., in background threads or non-HTTP scenarios), the method creates a new asynchronous
    /// service scope from the application's root <see cref="IServiceProvider"/>. The caller is responsible for disposing of this
    /// scope if the returned <c>IsOwned</c> value is <see langword="true"/> to avoid resource leaks.
    /// </para>
    /// </remarks>
    /// <returns>
    /// A tuple containing the resolved <see cref="IServiceProvider"/> and a <see cref="bool"/> indicating whether the caller owns
    /// the scope and is responsible for its disposal.
    /// </returns>
    public (IServiceProvider ServiceProvider, bool IsOwned) GetOrCreate()
    {
        // Check if there's an active HTTP context.
        if (httpContextAccessor.HttpContext is not null)
        {
            // Use the service provider from the current HTTP context.
            return (httpContextAccessor.HttpContext.RequestServices, false);
        }

        // Create a new scoped service provider.
        return (rootServiceProvider.CreateAsyncScope().ServiceProvider, true);
    }

    /// <summary>
    /// Creates a new asynchronous service scope.
    /// </summary>
    /// <remarks>The created <see cref="AsyncServiceScope"/> provides a scoped service provider for resolving
    /// scoped services. It is the caller's responsibility to dispose of the returned scope to release
    /// resources.</remarks>
    /// <returns>An <see cref="AsyncServiceScope"/> that represents the new service scope.</returns>
    public AsyncServiceScope CreateAsyncScope() =>
        rootServiceProvider.CreateAsyncScope();
}