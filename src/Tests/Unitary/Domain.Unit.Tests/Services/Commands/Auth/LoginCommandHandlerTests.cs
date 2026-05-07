using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Features.Auth.Commands.Login;
using Services.Interfaces;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLogger;
    private readonly LoginCommandValidator _validator;
    private readonly AuthCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public LoginCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockJwtTokenGenerator = MockServicesHelper.CreateMockJwtTokenGenerator();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();
        _mockLogger = new Mock<ILogger<LoginCommandHandler>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new LoginCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AuthCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var existingUser = _userFixture.GenerateConfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);
        _mockUserManager.SetupCheckPasswordAsync(true);

        var command = _commandFixture.GenerateLoginCommand();
        command.Email = existingUser.Email;

        var handler = new LoginCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            _validator,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.access_token.Should().Be("mock_access_token");
        result.Data.refresh_token.Should().Be("mock_refresh_token");

        _mockJwtTokenGenerator.Verify(j => j.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
        _mockJwtTokenGenerator.Verify(j => j.GenerateRefreshTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockUserManager.SetupFindByEmailAsync(null);

        var command = _commandFixture.GenerateLoginCommand();

        var handler = new LoginCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            _validator,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockJwtTokenGenerator.Verify(j => j.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailNotConfirmed_ShouldReturnValidationFailure()
    {
        // Arrange
        var unconfirmedUser = _userFixture.GenerateUnconfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(unconfirmedUser);
        _mockUserManager.SetupCheckPasswordAsync(true);

        var command = _commandFixture.GenerateLoginCommand();
        command.Email = unconfirmedUser.Email;

        var handler = new LoginCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            _validator,
            _mockLogger.Object,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockJwtTokenGenerator.Verify(j => j.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
