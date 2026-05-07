using Identity.Model;
using Microsoft.Extensions.Options;
using Moq;
using Services.Interfaces;
using System.Threading.Tasks;

namespace Unit.Tests.Helpers;

public static class MockServicesHelper
{
    public static Mock<IJwtTokenGenerator> CreateMockJwtTokenGenerator()
    {
        var mock = new Mock<IJwtTokenGenerator>();

        // Setup default responses for token generation
        mock.Setup(j => j.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("mock_access_token");

        mock.Setup(j => j.GenerateRefreshTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("mock_refresh_token");

        return mock;
    }

    public static Mock<IEmailService> CreateMockEmailService()
    {
        var mock = new Mock<IEmailService>();

        // Setup default responses for email sending
        mock.Setup(e => e.SendConfirmationLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mock.Setup(e => e.SendPasswordResetLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    public static IOptions<JWTConfig> CreateMockJwtConfig()
    {
        var jwtConfig = new JWTConfig
        {
            RedirectUriEmailConfirm = "https://example.com/confirm",
            RedirectUriResetPassword = "https://example.com/reset",
            RedirectUriExternalLogin = "https://example.com/external",
            Secret = "mock_secret_key_for_testing_purposes_123456",
            Issuer = "mock_issuer",
            Audience = "mock_audience",
            MinutesValid = 60
        };

        return Options.Create(jwtConfig);
    }
}
