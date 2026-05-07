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
using Services.Features.Auth.Commands.ConfirmEmail;
using Services.Interfaces;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class ConfirmEmailCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly ConfirmEmailCommandValidator _validator;
    private readonly AuthCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public ConfirmEmailCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockJwtTokenGenerator = MockServicesHelper.CreateMockJwtTokenGenerator();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new ConfirmEmailCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AuthCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldConfirmEmail()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUnconfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);
        _mockUserManager.SetupConfirmEmailAsync(true);

        var command = _commandFixture.GenerateConfirmEmailCommand();
        command.Email = existingUser.Email;

        var jwtConfig = MockServicesHelper.CreateMockJwtConfig();

        var handler = new ConfirmEmailCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
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
        result.Data.Uri.ToString().Should().Contain("token=");

        _mockUserManager.Verify(um => um.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _mockJwtTokenGenerator.Verify(j => j.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByEmailAsync(null);

        var command = _commandFixture.GenerateConfirmEmailCommand();

        var jwtConfig = MockServicesHelper.CreateMockJwtConfig();

        var handler = new ConfirmEmailCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            jwtConfig,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldReturnValidationFailure()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUnconfirmedUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);
        _mockUserManager.SetupConfirmEmailAsync(false);

        var command = _commandFixture.GenerateConfirmEmailCommand();
        command.Email = existingUser.Email;

        var jwtConfig = MockServicesHelper.CreateMockJwtConfig();

        var handler = new ConfirmEmailCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            jwtConfig,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _mockJwtTokenGenerator.Verify(j => j.GenerateAccessTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
