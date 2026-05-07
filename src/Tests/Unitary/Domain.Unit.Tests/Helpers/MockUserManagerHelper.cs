using Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unit.Tests.Helpers;

public static class MockUserManagerHelper
{
    public static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        return mockUserManager;
    }

    public static void SetupFindByIdAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, ApplicationUser user)
    {
        mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
    }

    public static void SetupUpdateAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, bool success)
    {
        var result = success
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = "Update failed" });

        mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(result);
    }

    public static void SetupCheckPasswordAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, bool isValid)
    {
        mockUserManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(isValid);
    }

    public static void SetupChangePasswordAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, bool success)
    {
        var result = success
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = "Password change failed" });

        mockUserManager.Setup(um => um.ChangePasswordAsync(
            It.IsAny<ApplicationUser>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(result);
    }

    public static void SetupFindByEmailAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, ApplicationUser user)
    {
        mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
    }

    public static void SetupCreateAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, bool success)
    {
        var result = success
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = "User creation failed" });

        mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(result);
    }

    public static void SetupGenerateEmailConfirmationTokenAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, string token)
    {
        mockUserManager.Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(token);
    }

    public static void SetupConfirmEmailAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, bool success)
    {
        var result = success
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = "Email confirmation failed" });

        mockUserManager.Setup(um => um.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(result);
    }

    public static void SetupGeneratePasswordResetTokenAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, string token)
    {
        mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(token);
    }

    public static void SetupResetPasswordAsync(this Mock<UserManager<ApplicationUser>> mockUserManager, bool success)
    {
        var result = success
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = "Password reset failed" });

        mockUserManager.Setup(um => um.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(result);
    }
}
