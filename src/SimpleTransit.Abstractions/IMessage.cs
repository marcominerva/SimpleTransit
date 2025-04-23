namespace SimpleTransit;

/// <summary>
/// Represents a contract for a message within the SimpleTransit system.
/// </summary>
/// <remarks>
/// This interface serves as a marker or base contract for all message types in the system. 
/// It is intentionally left empty to allow flexibility in defining specific message types 
/// while enabling consistent handling of messages across the application. 
/// The purpose of this interface is to provide a common abstraction for messages, 
/// facilitating extensibility and decoupling in the system's architecture.
/// </remarks>
public interface IMessage;
