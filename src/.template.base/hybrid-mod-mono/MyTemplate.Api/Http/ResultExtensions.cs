using Microsoft.AspNetCore.Mvc;
using MyTemplate.Application.Common.Results;

namespace MyTemplate.Api.Http;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.Accepted => TypedResults.Accepted(string.Empty, result.Value),
            ResultStatus.Created => TypedResults.Created(string.Empty, result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(ToProblem(result)),
            ResultStatus.Validation => TypedResults.BadRequest(ToProblem(result)),
            ResultStatus.Conflict => TypedResults.Conflict(ToProblem(result)),
            _ => TypedResults.Problem("Unexpected error")
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
