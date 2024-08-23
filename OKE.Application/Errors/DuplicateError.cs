using FluentResults;

namespace OKE.Application.Errors;
public class DuplicateError(string message) : Error(message)
{
    public const string ErrorCode = "409";
}
