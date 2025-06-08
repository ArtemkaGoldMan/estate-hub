using CSharpFunctionalExtensions;

namespace EstateHub.SharedKernel;

public record Error(string Status, string Type, string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, string.Empty);
    public static readonly Error NullValue = new(400.ToString(), "Error.NullValue", string.Empty, "Null value was provided");

    public static implicit operator Result(Error error) => Result.Failure($"{error.Status}{TextDelimiters.Separator}{error.Type}{TextDelimiters.Separator}{error.Code}{TextDelimiters.Separator}{error.Description}");

    public Result ToResult() => Result.Failure($"{Status}{TextDelimiters.Separator}{Type}{TextDelimiters.Separator}{Code}{TextDelimiters.Separator}{Description}");
    public override string ToString() => $"{Status}{TextDelimiters.Separator}{Type}{TextDelimiters.Separator}{Code}{TextDelimiters.Separator}{Description}";
}

public static class ErrorExtensions
{
    public static Result<T> ToResult<T>(this Error error) => Result.Failure<T>($"{error.Code}{TextDelimiters.Separator}{error.Description}");
}
