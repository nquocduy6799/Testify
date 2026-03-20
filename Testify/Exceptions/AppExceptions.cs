/// <summary>
/// Thrown when a requested resource is not found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>
/// Thrown when a request is semantically invalid (business-rule violation).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public sealed class ValidationException(string message) : Exception(message);

/// <summary>
/// Thrown when the caller does not have permission to perform the operation.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenException(string message) : Exception(message);

/// <summary>
/// Thrown when there is a conflict with the current state of the resource.
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);
