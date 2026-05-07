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
using Services.Features.Account.Commands.UpdatePassword;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class UpdatePasswordCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly Mock<ILogger<UpdatePasswordCommandHandler>> _mockLogger;
    private readonly UpdatePasswordCommandValidator _validator;
    private readonly AccountCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public UpdatePasswordCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();
        _mockLogger = new Mock<ILogger<UpdatePasswordCommandHandler>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new UpdatePasswordCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AccountCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePassword()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        var userId = Guid.Parse(existingUser.Id);
        _mockUserManager.SetupFindByIdAsync(existingUser);
        _mockUserManager.SetupCheckPasswordAsync(true);
        _mockUserManager.SetupChangePasswordAsync(true);
        _mockUser.Setup(u => u.GetUserId()).Returns(userId);

        var command = _commandFixture.GenerateUpdatePasswordCommand();

        var handler = new UpdatePasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockLogger.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockUserManager.Verify(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByIdAsync(null);

        var command = _commandFixture.GenerateUpdatePasswordCommand();

        var handler = new UpdatePasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockLogger.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();

        _mockUserManager.Verify(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CurrentPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        var userId = Guid.Parse(existingUser.Id);
        _mockUserManager.SetupFindByIdAsync(existingUser);
        _mockUserManager.SetupCheckPasswordAsync(false);
        _mockUser.Setup(u => u.GetUserId()).Returns(userId);

        var command = _commandFixture.GenerateUpdatePasswordCommand();

        var handler = new UpdatePasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockLogger.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PasswordChangeFails_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        var userId = Guid.Parse(existingUser.Id);
        _mockUserManager.SetupFindByIdAsync(existingUser);
        _mockUserManager.SetupCheckPasswordAsync(true);
        _mockUserManager.SetupChangePasswordAsync(false);
        _mockUser.Setup(u => u.GetUserId()).Returns(userId);

        var command = _commandFixture.GenerateUpdatePasswordCommand();

        var handler = new UpdatePasswordCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockLogger.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _mockUserManager.Verify(um => um.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
