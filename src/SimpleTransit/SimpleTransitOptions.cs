using SimpleTransit.Abstractions;

namespace SimpleTransit;

internal class SimpleTransitOptions
{
    public UnhandledExceptionBehavior UnhandledExceptionBehavior { get; set; }
}
