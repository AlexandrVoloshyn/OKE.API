﻿using FluentResults;
using FluentValidation;
using MediatR;
using OKE.Application.Errors;

namespace OKE.API.Middlewares;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> Validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase<TResponse>, new()
{

    private static Dictionary<string, Func<string, Error>> ErrorMap =>
        new()
        {
            [DuplicateError.ErrorCode] = message => new DuplicateError(message),
            [NotFoundError.ErrorCode] = message => new NotFoundError(message)
        };

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            Validators.Select(validator => validator.ValidateAsync(context)));

        var failures = validationResults.SelectMany(r => r.Errors)
            .GroupBy(
            x => x.PropertyName,
            x => (x.ErrorCode, x.ErrorMessage),
            (propertyName, tuple) =>
            new
            {
                Key = propertyName,
                Values = tuple,
            })
            .ToDictionary(x => x.Key, x => x.Values);

        if (!failures.Any())
        {
            return await next();
        }

        var result = new TResponse();

        result.Reasons.AddRange(
            failures.SelectMany(x => x.Value)
            .Select(GetErrorInstance));

        return result;

        Error GetErrorInstance((string? ErrorCode, string ErrorMessage) tuple)
        {
            var (errorCode, errorMessage) = tuple;
            return string.IsNullOrEmpty(errorCode) || !ErrorMap.TryGetValue(errorCode, out var factoryFunc)
                ? new BadRequestError(errorMessage)
                : factoryFunc(errorMessage);
        }
    }
}
