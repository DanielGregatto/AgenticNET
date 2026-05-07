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
using Services.Features.Auth.Commands.Register;
using Services.Interfaces;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class RegisterCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly RegisterCommandValidator _validator;
    private readonly AuthCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public RegisterCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockEmailService = MockServicesHelper.CreateMockEmailService();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new RegisterCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AuthCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterUser()
    {
        // Arrange
        _mockUserManager.SetupFindByEmailAsync(null); // No existing user
        _mockUserManager.SetupCreateAsync(true);
        _mockUserManager.SetupGenerateEmailConfirmationTokenAsync("mock_token");

        var command = _commandFixture.GenerateRegisterCommand();

        var handler = new RegisterCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockEmailService.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.id.Should().NotBeEmpty();

        _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _mockUserManager.Verify(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
        _mockEmailService.Verify(e => e.SendConfirmationLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        _mockUserManager.SetupFindByEmailAsync(existingUser);

        var command = _commandFixture.GenerateRegisterCommand();
        command.Email = existingUser.Email;

        var handler = new RegisterCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockEmailService.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockEmailService.Verify(e => e.SendConfirmationLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreateUserFails_ShouldReturnValidationFailure()
    {
        // Arrange
        _mockUserManager.SetupFindByEmailAsync(null);
        _mockUserManager.SetupCreateAsync(false);

        var command = _commandFixture.GenerateRegisterCommand();

        var handler = new RegisterCommandHandler(
            _context,
            _mockUser.Object,
            _mockUserManager.Object,
            _mockEmailService.Object,
            _validator,
            _mockLocalizer.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _mockEmailService.Verify(e => e.SendConfirmationLinkAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
