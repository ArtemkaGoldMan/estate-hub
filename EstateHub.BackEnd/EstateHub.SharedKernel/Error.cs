using CSharpFunctionalExtensions;

namespace EstateHub.SharedKernel;

public record Error(string Status, string Type, string Code, string Description, string? UserMessage = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, string.Empty);
    public static readonly Error NullValue = new(400.ToString(), "Error.NullValue", string.Empty, "Null value was provided");

    public static implicit operator Result(Error error) => Result.Failure(error.ToString());

    public Result ToResult() => Result.Failure(ToString());
    
    // Include UserMessage in string representation so it can be preserved through Result pattern
    public override string ToString() => 
        string.IsNullOrEmpty(UserMessage) 
            ? $"{Status}{TextDelimiters.Separator}{Type}{TextDelimiters.Separator}{Code}{TextDelimiters.Separator}{Description}"
            : $"{Status}{TextDelimiters.Separator}{Type}{TextDelimiters.Separator}{Code}{TextDelimiters.Separator}{Description}{TextDelimiters.Separator}{UserMessage}";
    
    public string GetUserMessage() => UserMessage ?? Description;
}

public static class ErrorExtensions
{
    public static Result<T> ToResult<T>(this Error error) => Result.Failure<T>(error.ToString());
    
    public static Error WithUserMessage(this Error error, string userMessage) => error with { UserMessage = userMessage };
}
