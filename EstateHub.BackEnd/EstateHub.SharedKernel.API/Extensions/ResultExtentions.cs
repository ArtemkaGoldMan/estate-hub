using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace EstateHub.SharedKernel.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result) =>
        ToProblemDetailsInternal(result.IsSuccess, result.GetErrorObject());

    public static IResult ToProblemDetails<T>(this Result<T> result) =>
        ToProblemDetailsInternal(result.IsSuccess, result.GetErrorObject());

    private static IResult ToProblemDetailsInternal(bool isSuccess, Error error)
    {
        if (isSuccess)
            throw new InvalidOperationException("Can't convert success result to problem");

        int statusCode = int.TryParse(error.Status, out var status) && IsValidHttpStatusCode(status)
            ? status
            : StatusCodes.Status400BadRequest;

        var extensions = new Dictionary<string, object?>
        {
            {
                "error", new
                {
                    error.Code,
                    error.Description,
                }
            },
            { "userMessage", error.GetUserMessage() }
        };

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitleForStatusCode(statusCode),
            type: error.Type,
            extensions: extensions);
    }

    private static bool IsValidHttpStatusCode(int statusCode)
    {
        return statusCode >= 100 && statusCode < 600;
    }

    private static string GetTitleForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status410Gone => "Gone",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
    }
}
