using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyTemplate.Api.Shared.Results;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => Microsoft.AspNetCore.Http.TypedResults.Ok(result.Value),
            ResultStatus.Created => Microsoft.AspNetCore.Http.TypedResults.Created(string.Empty, result.Value),
            ResultStatus.NotFound => Microsoft.AspNetCore.Http.TypedResults.NotFound(ToProblem(result)),
            ResultStatus.Validation => Microsoft.AspNetCore.Http.TypedResults.BadRequest(ToProblem(result)),
            ResultStatus.Conflict => Microsoft.AspNetCore.Http.TypedResults.Conflict(ToProblem(result)),
            _ => Microsoft.AspNetCore.Http.TypedResults.Problem("Unexpected error")
        };
    }

    private static ProblemDetails ToProblem<T>(Result<T> result)
    {
        return new ProblemDetails
        {
            Title = result.Error?.Code ?? "Error",
            Detail = result.Error?.Message ?? "An unexpected error occurred.",
            Status = result.Status switch
            {
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Validation => StatusCodes.Status400BadRequest,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            }
        };
    }
}
