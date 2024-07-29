using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CSharp.RuntimeBinder;
using OKE.Application.Errors;

namespace OKE.API.Filters;

public class ResultFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context) =>
        context.Result = context.Result is { } value ? ModifyResult(value) : null;

    private IActionResult? ModifyResult(IActionResult value)
    {
        return value is not ObjectResult { Value: ResultBase result } ? value : MapValue(result);

        IActionResult MapValue(ResultBase result) =>
            result switch
            {
                { IsSuccess: true } when GetValue(result) is { } value => new OkObjectResult(value),
                { IsSuccess: true } => new OkResult(),
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is NotFoundError) =>
                    new NotFoundObjectResult(
                        string.Join(Environment.NewLine, result.Errors.OfType<NotFoundError>().Select(x => x.Message))),
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is ProcessingError) =>
                    new ObjectResult(
                        string.Join(Environment.NewLine, result.Errors.OfType<ProcessingError>().Select(x => x.Message)))
                    {
                        StatusCode = 500
                    },
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is DuplicateError) =>
                    new ObjectResult(
                        string.Join(Environment.NewLine, result.Errors.OfType<DuplicateError>().Select(x => x.Message)))
                    {
                        StatusCode = 409
                    },
                { IsSuccess: false, Errors: var errors } when errors.Any(r => r is BadRequestError) =>
                    new ObjectResult(
                        string.Join(Environment.NewLine, result.Errors.OfType<BadRequestError>().Select(x => x.Message)))
                    {
                        StatusCode = 400
                    },
                { IsSuccess: false } => new ObjectResult(string.Join(Environment.NewLine, result.Errors.Select(x => x.Message))) { StatusCode = 500 }
            };

    }

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

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Not used.
    }
}
