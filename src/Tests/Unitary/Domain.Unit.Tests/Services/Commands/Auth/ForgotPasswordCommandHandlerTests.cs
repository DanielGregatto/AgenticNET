using System;
using System.Threading;
using System.Threading.Tasks;
using Data.Context;
using Domain.Interfaces;
using FluentAssertions;
using Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Moq;
using Services.Features.Auth.Commands.ForgotPassword;
using Services.Interfaces;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class ForgotPasswordCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly ForgotPasswordCommandValidator _validator;
    private readonly AuthCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public ForgotPasswordCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockEmailService = MockServicesHelper.CreateMockEmailService();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new ForgotPasswordCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AuthCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidEmail_ShouldSendResetLink()
    {
        // Arrange
        var existingUser = _userFixture.GenerateConfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);
        _mockUserManager.SetupGeneratePasswordResetTokenAsync("mock_reset_token");

        var command = _commandFixture.GenerateForgotPasswordCommand();
        command.Email = existingUser.Email;

        var jwtConfig = MockServicesHelper.CreateMockJwtConfig();

        var handler = new ForgotPasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockEmailService.Object,
            jwtConfig,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.token.Should().Be("mock_reset_token");

        _mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
        _mockEmailService.Verify(e => e.SendPasswordResetLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByEmailAsync(null);

        var command = _commandFixture.GenerateForgotPasswordCommand();

        var jwtConfig = MockServicesHelper.CreateMockJwtConfig();

        var handler = new ForgotPasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockEmailService.Object,
            jwtConfig,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _mockEmailService.Verify(e => e.SendPasswordResetLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailNotConfirmed_ShouldReturnValidationFailure()
    {
        // Arrange
        var unconfirmedUser = _userFixture.GenerateUnconfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(unconfirmedUser);

        var command = _commandFixture.GenerateForgotPasswordCommand();
        command.Email = unconfirmedUser.Email;

        var jwtConfig = MockServicesHelper.CreateMockJwtConfig();

        var handler = new ForgotPasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockEmailService.Object,
            jwtConfig,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _mockEmailService.Verify(e => e.SendPasswordResetLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
