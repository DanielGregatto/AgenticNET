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
using Services.Features.Account.Commands.UpdatePersonalInfo;
using Unit.Tests.Fixtures;
using Unit.Tests.Helpers;
using Util.Interfaces;
using Xunit;

namespace Unit.Tests.Services.Commands;

public class UpdatePersonalInfoCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUser> _mockUser;
    private readonly Mock<IStringLocalizer<Domain.Resources.Messages>> _mockLocalizer;
    private readonly Mock<IHandlerValidation> _mockHandlerValidation;
    private readonly Mock<ILogger<UpdatePersonalInfoCommandHandler>> _mockLogger;
    private readonly UpdatePersonalInfoCommandValidator _validator;
    private readonly AccountCommandFixture _commandFixture;
    private readonly ApplicationUserFixture _userFixture;

    public UpdatePersonalInfoCommandHandlerTests()
    {
        _context = MockDbContextHelper.CreateInMemoryDbContext();
        _mockUserManager = MockUserManagerHelper.CreateMockUserManager();
        _mockUser = new Mock<IUser>();
        _mockLocalizer = new Mock<IStringLocalizer<Domain.Resources.Messages>>();
        _mockHandlerValidation = new Mock<IHandlerValidation>();
        _mockLogger = new Mock<ILogger<UpdatePersonalInfoCommandHandler>>();

        // Setup localizer to return non-null strings
        _mockLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        // Setup handler validation to return true for valid phone and CPF/CNPJ
        _mockHandlerValidation.Setup(h => h.IsValidPhoneNumber(It.IsAny<string>())).Returns(true);
        _mockHandlerValidation.Setup(h => h.IsValidCPFOrCNPJ(It.IsAny<string>())).Returns(true);

        _validator = new UpdatePersonalInfoCommandValidator(_mockLocalizer.Object, _mockHandlerValidation.Object);
        _commandFixture = new AccountCommandFixture();
        _userFixture = new ApplicationUserFixture();

        _mockUser.Setup(u => u.GetUserId()).Returns(Guid.NewGuid());
        _mockUser.Setup(u => u.IsAuthenticated()).Returns(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePersonalInfo()
    {
        // Arrange
        var existingUser = _userFixture.GenerateUser();
        var userId = Guid.Parse(existingUser.Id);
        _mockUserManager.SetupFindByIdAsync(existingUser);
        _mockUserManager.SetupUpdateAsync(true);
        _mockUser.Setup(u => u.GetUserId()).Returns(userId);

        var command = _commandFixture.GenerateUpdatePersonalInfoCommand();

        var handler = new UpdatePersonalInfoCommandHandler(
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
        result.Data.FullName.Should().Be(command.FullName);
        result.Data.PhoneNumber.Should().Be(command.PhoneNumber);
        result.Data.CPF_CNPJ.Should().Be(command.CPF_CNPJ);
        result.Data.DateOfBirth.Should().Be(command.DateOfBirth);

        _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockUserManager.SetupFindByIdAsync(null);

        var command = _commandFixture.GenerateUpdatePersonalInfoCommand();

        var handler = new UpdatePersonalInfoCommandHandler(
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

        var command = _commandFixture.GenerateUpdatePersonalInfoCommand();

        var handler = new UpdatePersonalInfoCommandHandler(
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
