using System;

using dailycue_api.Exceptions.CustomExceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace dailycue_api.Exceptions.Handlers;

public class BadRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadRequestException badRequestException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = badRequestException.Message,
            Instance = httpContext.Request.Path
        };
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(value: problemDetails, options: null, type: typeof(ProblemDetails), cancellationToken: cancellationToken);
        return true;
    }
}