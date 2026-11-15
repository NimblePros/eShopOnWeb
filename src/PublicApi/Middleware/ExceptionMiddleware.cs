using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.PublicApi.Middleware;

public class ExceptionMiddleware
{
    public const string UnexpectedErrorMessage = "An unexpected error occurred.";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.ContentType = "application/json";

        if (exception is DuplicateException || exception is RoleStillAssignedException)
        {
            // Expected domain conflicts: their messages are written for API callers.
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                TraceId = traceId
            }.ToString());
        }
        else
        {
            // Unexpected failures can carry internal details (SQL, type names, usernames),
            // so log them server-side and return only a generic message and trace id (CWE-209).
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = UnexpectedErrorMessage,
                TraceId = traceId
            }.ToString());
        }
    }
}
