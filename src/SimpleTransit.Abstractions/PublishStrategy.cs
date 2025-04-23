namespace SimpleTransit;

/// <summary>
/// Defines the strategy to be used when publishing messages to multiple subscribers.
/// </summary>
/// <remarks>
/// This enumeration provides two distinct strategies for handling asynchronous operations
/// when publishing messages:
/// <list type="bullet">
/// <item>
/// <term>AwaitForEach</term>
/// <description>
/// Executes each subscriber's handler sequentially, awaiting the completion of each one
/// before moving to the next. This ensures that handlers are processed in order but may
/// result in slower overall execution.
/// </description>
/// </item>
/// <item>
/// <term>AwaitWhenAll</term>
/// <description>
/// Executes all subscriber handlers concurrently and awaits their completion as a group.
/// This approach is faster for independent handlers but does not guarantee order of execution.
/// </description>
/// </item>
/// </list>
/// </remarks>
public enum PublishStrategy
{
    /// <summary>
    /// Executes each subscriber's handler sequentially, awaiting each one before proceeding.
    /// </summary>
    AwaitForEach,

    /// <summary>
    /// Executes all subscriber handlers concurrently and awaits their completion as a group.
    /// </summary>
    AwaitWhenAll
}
