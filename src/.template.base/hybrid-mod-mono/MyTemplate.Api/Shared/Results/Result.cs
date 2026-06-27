namespace MyTemplate.Api.Shared.Results;

public class Result<T>
{
    public bool IsSuccess => Status is ResultStatus.Ok or ResultStatus.Created;
    public T? Value { get; }
    public Error? Error { get; }
    public ResultStatus Status { get; }

    protected Result(T? value, Error? error, ResultStatus status)
    {
        Value = value;
        Error = error;
        Status = status;
    }

    public static Result<T> Ok(T value) => new(value, null, ResultStatus.Ok);
    public static Result<T> Created(T value) => new(value, null, ResultStatus.Created);
    public static Result<T> NotFound(string message) => new(default, new Error("NotFound", message), ResultStatus.NotFound);
    public static Result<T> Validation(string message) => new(default, new Error("Validation", message), ResultStatus.Validation);
    public static Result<T> Conflict(string message) => new(default, new Error("Conflict", message), ResultStatus.Conflict);
    public static Result<T> Failure(string code, string message) => new(default, new Error(code, message), ResultStatus.Error);
}

public class Result : Result<string>
{
    protected Result(string? value, Error? error, ResultStatus status) : base(value, error, status) { }
    
    public static Result Success() => new(string.Empty, null, ResultStatus.Ok);
    public static new Result NotFound(string message) => new(string.Empty, new Error("NotFound", message), ResultStatus.NotFound);
    public static new Result Validation(string message) => new(string.Empty, new Error("Validation", message), ResultStatus.Validation);
    public static new Result Conflict(string message) => new(string.Empty, new Error("Conflict", message), ResultStatus.Conflict);
    public static new Result Failure(string code, string message) => new(string.Empty, new Error(code, message), ResultStatus.Error);
}
