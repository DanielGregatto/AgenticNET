using Bogus;
using Services.Features.Auth.Commands.ConfirmEmail;
using Services.Features.Auth.Commands.ForgotPassword;
using Services.Features.Auth.Commands.Login;
using Services.Features.Auth.Commands.Register;
using Services.Features.Auth.Commands.ResetPassword;

namespace Unit.Tests.Fixtures;

public class AuthCommandFixture
{
    private readonly Faker _faker;

    public AuthCommandFixture()
    {
        _faker = new Faker();
    }

    public LoginCommand GenerateLoginCommand()
    {
        return new LoginCommand
        {
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(6)
        };
    }

    public RegisterCommand GenerateRegisterCommand()
    {
        var password = _faker.Internet.Password(6);

        return new RegisterCommand
        {
            Email = _faker.Internet.Email(),
            Password = password,
            ConfirmPassword = password,
            ConfirmationBaseUrl = _faker.Internet.Url()
        };
    }

    public ConfirmEmailCommand GenerateConfirmEmailCommand()
    {
        return new ConfirmEmailCommand
        {
            Email = _faker.Internet.Email(),
            Token = _faker.Random.AlphaNumeric(64)
        };
    }

    public ForgotPasswordCommand GenerateForgotPasswordCommand()
    {
        return new ForgotPasswordCommand
        {
            Email = _faker.Internet.Email()
        };
    }

    public ResetPasswordCommand GenerateResetPasswordCommand()
    {
        var newPassword = _faker.Internet.Password(6);

        return new ResetPasswordCommand
        {
            Email = _faker.Internet.Email(),
            Token = _faker.Random.AlphaNumeric(64),
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };
    }
}
