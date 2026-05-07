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
using Services.Features.Account.Commands.UpdateAddress;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class UpdateAddressCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly Mock<ILogger<UpdateAddressCommandHandler>> _mockLogger;
    private readonly UpdateAddressCommandValidator _validator;
    private readonly AccountCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public UpdateAddressCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();
        _mockLogger = new Mock<ILogger<UpdateAddressCommandHandler>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _validator = new UpdateAddressCommandValidator(_mockLocalizer.Object);
        _commandFixture = new AccountCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateAddress()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        var userId = Guid.Parse(existingUser.Id);
        _mockUserManager.SetupFindByIdAsync(existingUser);
        _mockUserManager.SetupUpdateAsync(true);
        _mockUser.Setup(u => u.GetUserId()).Returns(userId);

        var command = _commandFixture.GenerateUpdateAddressCommand();

        var handler = new UpdateAddressCommandHandler(
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
        result.Data.Id.Should().Be(userId);
        result.Data.Street.Should().Be(command.Street);
        result.Data.City.Should().Be(command.City);
        result.Data.State.Should().Be(command.State);

        _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByIdAsync(null);

        var command = _commandFixture.GenerateUpdateAddressCommand();

        var handler = new UpdateAddressCommandHandler(
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

        _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        var userId = Guid.Parse(existingUser.Id);
        _mockUserManager.SetupFindByIdAsync(existingUser);
        _mockUserManager.SetupUpdateAsync(false);
        _mockUser.Setup(u => u.GetUserId()).Returns(userId);

        var command = _commandFixture.GenerateUpdateAddressCommand();

        var handler = new UpdateAddressCommandHandler(
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

        _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
