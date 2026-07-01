namespace MyTemplate.Application.Common.Results;

public sealed class Result<T>
{
    private Result(T? value, Error? error, ResultStatus status)
    {
        Value = value;
        Error = error;
        Status = status;
    }

    public bool IsSuccess => Status is ResultStatus.Ok or ResultStatus.Accepted or ResultStatus.Created;
    public T? Value { get; }
    public Error? Error { get; }
    public ResultStatus Status { get; }

    public static Result<T> Ok(T value) => new(value, null, ResultStatus.Ok);

    public static Result<T> Accepted(T value) => new(value, null, ResultStatus.Accepted);

    public static Result<T> Created(T value) => new(value, null, ResultStatus.Created);

    public static Result<T> NotFound(string code, string message) =>
        new(default, new Error(code, message), ResultStatus.NotFound);

    public static Result<T> Validation(string code, string message) =>
        new(default, new Error(code, message), ResultStatus.Validation);

    public static Result<T> Conflict(string code, string message) =>
        new(default, new Error(code, message), ResultStatus.Conflict);

    public static Result<T> Failure(string code, string message) =>
        new(default, new Error(code, message), ResultStatus.Error);
}
