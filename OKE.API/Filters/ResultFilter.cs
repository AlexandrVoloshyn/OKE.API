using FluentResults;
using Microsoft.CSharp.RuntimeBinder;
using OKE.Application.Errors;

namespace OKE.API.Filters;

public class ResultFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        if (result is ResultBase baseResult)
        {
            return ModifyResult(baseResult);
        }
        return result;
    }

    private static IResult ModifyResult(ResultBase result) =>
            result switch
            {
                { IsSuccess: true } when GetValue(result) is { } value => TypedResults.Ok(value),
                { IsSuccess: true } => TypedResults.Ok(),
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is NotFoundError) =>
                    TypedResults.NotFound(
                        new DetailedError(result.Errors.Select(x => x.Message))),
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is DuplicateError) =>
                    TypedResults.Conflict(
                        new DetailedError(result.Errors.Select(x => x.Message))),
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is BadRequestError) =>
                    TypedResults.BadRequest(
                        new DetailedError(result.Errors.Select(x => x.Message))),
                { IsSuccess: false } =>
                    Results.Json(new DetailedError(result.Errors.Select(x => x.Message)), statusCode: 500)
            };

    static object? GetValue(ResultBase result)
    {
        dynamic res = result;
        try
        {
            return res.Value;
        }
        catch (RuntimeBinderException)
        {
            return null;
        }
    }
}

public record DetailedError(IEnumerable<string> Messages);