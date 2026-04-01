using Ardalis.Result;

namespace Common.LanguageExtensions.Utilities.ResultExtensions;

public static partial class ResultFunctionalExtensions
{
    public static Result<K> AsTypedError<K>(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("can only cast with error result");
        }

        var errors = GetAllErrors(result.Errors, result.ValidationErrors);
        return ConvertToTypedResult<K>(result.Status, errors);
    }

    public static Result<K> AsTypedError<T, K>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("can only cast with error result");
        }

        var errors = GetAllErrors(result.Errors, result.ValidationErrors);
        return ConvertToTypedResult<K>(result.Status, errors);
    }

    public static Result AsResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Result.Success();
        }

        var errors = GetAllErrors(result.Errors, result.ValidationErrors);
        return ConvertToUntypedResult(result.Status, errors);
    }

    public static async Task<Result> AsResult<T>(this Task<Result<T>> resultTask)
    {
        var result = await resultTask;

        if (result.IsSuccess)
        {
            return Result.Success();
        }

        return Result.CriticalError([.. result.Errors]);
    }

    private static IReadOnlyCollection<string> GetAllErrors(
        IEnumerable<string> errors,
        IEnumerable<ValidationError> validationErrors)
    {
        return errors
            .Concat(validationErrors.Select(e => e.ErrorMessage))
            .ToList();
    }

    private static Result<K> ConvertToTypedResult<K>(ResultStatus status, IReadOnlyCollection<string> errors)
    {
        var errorMessage = errors.Count == 1 ? errors.Single() : string.Join(",", errors);

        return status switch
        {
            ResultStatus.Conflict => Result.Conflict(errorMessage),
            ResultStatus.Error => errors.Count == 1
                ? Result.Error(errorMessage)
                : Result.Error(new ErrorList(errors)),
            ResultStatus.Forbidden => Result.Forbidden(errorMessage),
            ResultStatus.Invalid => Result.Invalid(new ValidationError(errorMessage)),
            ResultStatus.NotFound => Result.NotFound(errorMessage),
            ResultStatus.Unauthorized => Result.Unauthorized(errorMessage),
            ResultStatus.Unavailable => errors.Count == 1
                ? Result.Unavailable(errorMessage)
                : Result.Unavailable([.. errors]),
            _ => errors.Count == 1
                ? Result.CriticalError(errorMessage)
                : Result.CriticalError([.. errors])
        };
    }

    private static Result ConvertToUntypedResult(ResultStatus status, IReadOnlyCollection<string> errors)
    {
        var errorMessage = errors.Count == 1 ? errors.Single() : string.Join(",", errors);

        return status switch
        {
            ResultStatus.Conflict => Result.Conflict(errorMessage),
            ResultStatus.Error => errors.Count == 1
                ? Result.Error(errorMessage)
                : Result.Error(new ErrorList(errors)),
            ResultStatus.Forbidden => Result.Forbidden(errorMessage),
            ResultStatus.Invalid => Result.Invalid(new ValidationError(errorMessage)),
            ResultStatus.NotFound => Result.NotFound(errorMessage),
            ResultStatus.Unauthorized => Result.Unauthorized(errorMessage),
            ResultStatus.Unavailable => errors.Count == 1
                ? Result.Unavailable(errorMessage)
                : Result.Unavailable([.. errors]),
            _ => errors.Count == 1
                ? Result.CriticalError(errorMessage)
                : Result.CriticalError([.. errors])
        };
    }
}
