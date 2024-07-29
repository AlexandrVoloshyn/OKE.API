using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKE.Application.Errors;
public class NotFoundError : Error
{
    public const string ErrorCode = "404";

    public NotFoundError(string message) : base(message)
    {
    }
}
