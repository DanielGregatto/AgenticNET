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
using Services.Features.Auth.Commands.ResetPassword;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class ResetPasswordCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly ResetPasswordCommandValidator _validator;
    private readonly AuthCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public ResetPasswordCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new ResetPasswordCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AuthCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldResetPassword()
    {
        // Arrange
        var existingUser = _userFixture.GenerateConfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);
        _mockUserManager.SetupResetPasswordAsync(true);

        var command = _commandFixture.GenerateResetPasswordCommand();
        command.Email = existingUser.Email;

        var handler = new ResetPasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockUserManager.Verify(um => um.ResetPasswordAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByEmailAsync(null);

        var command = _commandFixture.GenerateResetPasswordCommand();

        var handler = new ResetPasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.ResetPasswordAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldReturnValidationFailure()
    {
        // Arrange
        var existingUser = _userFixture.GenerateConfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);
        _mockUserManager.SetupResetPasswordAsync(false);

        var command = _commandFixture.GenerateResetPasswordCommand();
        command.Email = existingUser.Email;

        var handler = new ResetPasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.ResetPasswordAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
