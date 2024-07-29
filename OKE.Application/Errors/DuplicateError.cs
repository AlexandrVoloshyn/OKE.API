using FluentResults;

namespace OKE.Application.Errors;
public class DuplicateError : Error
{
    public const string ErrorCode = "409";
    public DuplicateError(string message) : base(message)
    {
    }
}
