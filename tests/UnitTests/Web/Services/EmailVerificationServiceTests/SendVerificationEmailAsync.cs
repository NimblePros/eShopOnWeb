using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Services;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Web.Services.EmailVerificationServiceTests;

public class SendVerificationEmailAsync
{
    private const string Token = "confirmation-token";
    private const string CallbackUrl = "https://localhost/ConfirmEmail?userId=user-id&code=confirmation-token";
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
        _urlHelper.Action(Arg.Any<UrlActionContext>()).Returns(CallbackUrl);
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

        _urlHelper.Received(1).Action(Arg.Is<UrlActionContext>(ctx =>
            ctx.Controller == "ConfirmEmail" &&
            ctx.Protocol == Scheme &&
            ctx.Values != null &&
            ctx.Values.ToString()!.Contains(_user.Id) &&
            ctx.Values.ToString()!.Contains(Token)));
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
        _urlHelper.Action(Arg.Any<UrlActionContext>()).Returns((string?)null);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.SendVerificationEmailAsync(_user, _urlHelper, Scheme));

        await _emailSender.DidNotReceiveWithAnyArgs().SendEmailAsync(default!, default!, default!);
    }
}
