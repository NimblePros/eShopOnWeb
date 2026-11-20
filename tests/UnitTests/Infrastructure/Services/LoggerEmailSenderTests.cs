using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Infrastructure.Services;

public class LoggerEmailSenderTests
{
    private const string Email = "jane.doe@example.com";
    private const string Subject = "Confirm your email";
    private const string Body = "Please confirm your account by clicking this link: <a href='https://shop/confirm?code=SECRET-TOKEN'>link</a>";

    private readonly IAppLogger<LoggerEmailSender> _logger = Substitute.For<IAppLogger<LoggerEmailSender>>();

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task DoesNotLogBodyOrFullAddressOutsideLocalDevelopment(string environmentName)
    {
        var sender = new LoggerEmailSender(_logger, CreateEnvironment(environmentName));

        await sender.SendEmailAsync(Email, Subject, Body);

        var call = Assert.Single(_logger.ReceivedCalls());
        var loggedValues = string.Join(" | ", call.GetArguments()[0], string.Join(" | ", (object[])call.GetArguments()[1]!));
        Assert.DoesNotContain(Email, loggedValues);
        Assert.DoesNotContain("SECRET-TOKEN", loggedValues);
        Assert.DoesNotContain(Body, loggedValues);
        _logger.Received(1).LogInformation(Arg.Any<string>(),
            Arg.Is<object[]>(args => args.Length == 2 && (string)args[0] == "j***@example.com" && (string)args[1] == Subject));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Docker")]
    [InlineData("Testing")]
    public async Task LogsFullEmailInLocalDevelopment(string environmentName)
    {
        var sender = new LoggerEmailSender(_logger, CreateEnvironment(environmentName));

        await sender.SendEmailAsync(Email, Subject, Body);

        _logger.Received(1).LogInformation(Arg.Any<string>(),
            Arg.Is<object[]>(args => args.Length == 3 && (string)args[0] == Email && (string)args[1] == Subject && (string)args[2] == Body));
    }

    [Theory]
    [InlineData("jane.doe@example.com", "j***@example.com")]
    [InlineData("a@b.co", "a***@b.co")]
    [InlineData("not-an-email", "***")]
    [InlineData("@example.com", "***")]
    [InlineData("jane@", "***")]
    [InlineData("", "***")]
    [InlineData(null, "***")]
    public void MaskEmailHidesTheAddress(string? email, string expected)
    {
        Assert.Equal(expected, LoggerEmailSender.MaskEmail(email));
    }

    private static IHostEnvironment CreateEnvironment(string environmentName)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);
        return environment;
    }
}
