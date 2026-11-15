using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.PublicApi.Middleware;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.Middleware;

[TestClass]
public class ExceptionMiddlewareTest
{
    [TestMethod]
    public async Task UnexpectedExceptionReturns500WithoutExceptionMessage()
    {
        const string secret = "Invalid column name 'PasswordHash' in table 'AspNetUsers'";

        var (context, body) = await InvokeWith(new InvalidOperationException(secret));

        Assert.AreEqual((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.IsFalse(body.Contains(secret), "500 response body must not contain the original exception message.");

        var details = Deserialize(body);
        Assert.AreEqual(500, details.StatusCode);
        Assert.AreEqual(ExceptionMiddleware.UnexpectedErrorMessage, details.Message);
        Assert.AreEqual(context.TraceIdentifier, details.TraceId);
    }

    [TestMethod]
    public async Task DuplicateExceptionReturns409WithMessage()
    {
        var (context, body) = await InvokeWith(new DuplicateException("A role named 'Admin' already exists."));

        Assert.AreEqual((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        var details = Deserialize(body);
        Assert.AreEqual(409, details.StatusCode);
        Assert.AreEqual("A role named 'Admin' already exists.", details.Message);
    }

    [TestMethod]
    public async Task RoleStillAssignedExceptionReturns409WithMessage()
    {
        var exception = new RoleStillAssignedException("Role 'Product Managers' is still assigned to users.");

        var (context, body) = await InvokeWith(exception);

        Assert.AreEqual((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        Assert.AreEqual(exception.Message, Deserialize(body).Message);
    }

    private static async Task<(HttpContext Context, string Body)> InvokeWith(Exception exception)
    {
        var context = new DefaultHttpContext { TraceIdentifier = "test-trace-id" };
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionMiddleware(_ => throw exception, NullLogger<ExceptionMiddleware>.Instance);
        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return (context, await reader.ReadToEndAsync());
    }

    private static ErrorDetails Deserialize(string body)
    {
        var details = JsonSerializer.Deserialize<ErrorDetails>(body);
        Assert.IsNotNull(details);
        return details;
    }
}
