using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Services;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Web.Services.EmailVerificationServiceTests;

public class SendVerificationEmailAsync
{
    private const string Token = "confirmation-token";
    private const string CallbackUrl = "https://localhost/Identity/Account/ConfirmEmail?userId=user-id&code=confirmation-token";
    private const string Scheme = "https";

    private readonly UserManager<ApplicationUser> _userManager =
        Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IUrlHelper _urlHelper = Substitute.For<IUrlHelper>();
    private readonly EmailVerificationService _service;

    private readonly ApplicationUser _user = new()
    {
        Id = "user-id",
        UserName = "demouser",
        Email = "demouser@microsoft.com"
    };

    public SendVerificationEmailAsync()
    {
        _userManager.GenerateEmailConfirmationTokenAsync(_user).Returns(Token);
        _urlHelper.ActionContext.Returns(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()));
        _urlHelper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns(CallbackUrl);
        _service = new EmailVerificationService(_userManager, _emailSender);
    }

    [Fact]
    public async Task SendsConfirmationEmailToUserWithCallbackLink()
    {
        await _service.SendVerificationEmailAsync(_user, _urlHelper, Scheme);

        await _emailSender.Received(1).SendEmailAsync(
            _user.Email!,
            "Confirm your email",
            Arg.Is<string>(body => body.Contains(HtmlEncoder.Default.Encode(CallbackUrl))));
    }

    [Fact]
    public async Task BuildsConfirmationLinkFromUserIdTokenAndScheme()
    {
        await _service.SendVerificationEmailAsync(_user, _urlHelper, Scheme);

        _urlHelper.Received(1).RouteUrl(Arg.Is<UrlRouteContext>(ctx =>
            ctx.Protocol == Scheme &&
            ctx.Values != null &&
            HasRouteValues(ctx.Values)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ThrowsAndSendsNothingWhenUserHasNoEmail(string? email)
    {
        _user.Email = email;

        await Assert.ThrowsAsync<ApplicationException>(
            () => _service.SendVerificationEmailAsync(_user, _urlHelper, Scheme));

        await _userManager.DidNotReceive().GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>());
        await _emailSender.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, default!);
    }

    [Fact]
    public async Task ThrowsAndSendsNothingWhenCallbackUrlCannotBeGenerated()
    {
        _urlHelper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns((string?)null);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.SendVerificationEmailAsync(_user, _urlHelper, Scheme));

        await _emailSender.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, default!);
    }

    private bool HasRouteValues(object values)
    {
        var routeValues = new RouteValueDictionary(values);
        return Equals(routeValues["page"], "/Account/ConfirmEmail") &&
            Equals(routeValues["area"], "Identity") &&
            Equals(routeValues["userId"], _user.Id) &&
            Equals(routeValues["code"], Token);
    }
}
