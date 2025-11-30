using CSharpFunctionalExtensions;

namespace EstateHub.SharedKernel;

public static class ResultExtensions
{
    public static Error GetErrorObject(this Result result) =>
        GetErrorObjectInternal(result.IsSuccess, result.Error);

    public static Error GetErrorObject<T>(this Result<T> result) =>
        GetErrorObjectInternal(result.IsSuccess, result.Error);

    private static Error GetErrorObjectInternal(bool isSuccess, string errorString)
    {
        if (isSuccess)
            throw new InvalidOperationException("Can't convert success result to error");

        if (string.IsNullOrEmpty(errorString))
            return Error.None;

        string[] parts = errorString.Split(TextDelimiters.Separator);
        if (parts.Length < 4)
            return Error.None;

        // Parse UserMessage if present (5th part)
        string? userMessage = parts.Length > 4 ? parts[4] : null;

        return new Error(
            parts[0],
            parts[1],
            parts[2],
            parts[3],
            userMessage);
    }

    public static Result<T> Failure<T>(Error error) => Result.Failure<T>(error.ToString());
    public static Result Failure(Error error) => Result.Failure(error.ToString());

}
