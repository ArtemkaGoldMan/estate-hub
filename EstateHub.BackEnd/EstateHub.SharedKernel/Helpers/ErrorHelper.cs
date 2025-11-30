namespace EstateHub.SharedKernel.Helpers;

/// <summary>
/// Simple helper to throw exceptions with Error objects for user-friendly messages
/// </summary>
public static class ErrorHelper
{
    /// <summary>
    /// Throws ArgumentException with Error stored in Data for GlobalExceptionHandler to extract
    /// </summary>
    public static void ThrowError(Error error)
    {
        throw new ArgumentException(error.ToString()) { Data = { ["Error"] = error } };
    }

    /// <summary>
    /// Throws ArgumentNullException with Error stored in Data
    /// </summary>
    public static void ThrowErrorNull(Error error)
    {
        throw new ArgumentNullException(error.ToString()) { Data = { ["Error"] = error } };
    }

    /// <summary>
    /// Throws InvalidOperationException with Error stored in Data
    /// </summary>
    public static void ThrowErrorOperation(Error error)
    {
        throw new InvalidOperationException(error.ToString()) { Data = { ["Error"] = error } };
    }
}

