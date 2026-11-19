using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlazorAdmin.Services;
using BlazorShared;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.BlazorAdmin.Services;

public class HttpServiceTests
{
    private const string FastEndpointsValidationError =
        """{"statusCode":400,"message":"One or more errors occurred!","errors":{"generalErrors":["Username 'bad name' is invalid, can only contain letters or digits."]}}""";

    [Fact]
    public async Task HttpPostShowsFirstValidationErrorInsteadOfGenericMessage()
    {
        var (service, toasts) = CreateService(HttpStatusCode.BadRequest, FastEndpointsValidationError);

        var result = await service.HttpPost<object>("users", new { });

        Assert.Null(result);
        Assert.Equal(("Username 'bad name' is invalid, can only contain letters or digits.", ToastLevel.Error), Assert.Single(toasts.Shown));
    }

    [Fact]
    public async Task HttpPutShowsFirstValidationError()
    {
        var (service, toasts) = CreateService(HttpStatusCode.BadRequest, FastEndpointsValidationError);

        var result = await service.HttpPut<object>("users", new { });

        Assert.Null(result);
        Assert.Equal("Username 'bad name' is invalid, can only contain letters or digits.", Assert.Single(toasts.Shown).Message);
    }

    [Fact]
    public async Task HttpPutWithoutResultShowsFirstValidationError()
    {
        var (service, toasts) = CreateService(HttpStatusCode.BadRequest, FastEndpointsValidationError);

        var status = await service.HttpPut("users/1/roles", new { });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal("Username 'bad name' is invalid, can only contain letters or digits.", Assert.Single(toasts.Shown).Message);
    }

    [Fact]
    public async Task HttpPostShowsMessageGivenExceptionMiddlewareError()
    {
        // ExceptionMiddleware serializes ErrorDetails with PascalCase names and no errors.
        var (service, toasts) = CreateService(HttpStatusCode.Conflict, """{"StatusCode":409,"Message":"User already exists.","TraceId":"abc"}""");

        await service.HttpPost<object>("users", new { });

        Assert.Equal("User already exists.", Assert.Single(toasts.Shown).Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[]")]
    public async Task HttpPutShowsStatusCodeGivenNoErrorMessageInBody(string body)
    {
        var (service, toasts) = CreateService(HttpStatusCode.NotFound, body);

        var result = await service.HttpPut<object>("users", new { });

        Assert.Null(result);
        Assert.Equal("The request failed (404).", Assert.Single(toasts.Shown).Message);
    }

    [Fact]
    public async Task HttpDeleteShowsMessageGivenError()
    {
        var (service, toasts) = CreateService(HttpStatusCode.BadRequest, """{"statusCode":400,"message":"The default admin user cannot be deleted."}""");

        await service.HttpDelete("users/1");

        Assert.Equal("The default admin user cannot be deleted.", Assert.Single(toasts.Shown).Message);
    }

    private static (HttpService Service, ToastRecorder Toasts) CreateService(HttpStatusCode statusCode, string body)
    {
        var httpClient = new HttpClient(new StubHandler(statusCode, body));
        var options = Options.Create(new BaseUrlConfiguration { ApiBase = "https://api.test/" });
        var toastService = new ToastService();
        return (new HttpService(httpClient, options, toastService), new ToastRecorder(toastService));
    }

    private sealed class ToastRecorder
    {
        public List<(string Message, ToastLevel Level)> Shown { get; } = [];

        public ToastRecorder(ToastService toastService)
        {
            toastService.OnShow += (message, level) => Shown.Add((message, level));
        }
    }

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
    }
}
