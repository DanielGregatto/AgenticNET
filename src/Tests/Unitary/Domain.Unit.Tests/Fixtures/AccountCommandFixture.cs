using Bogus;
using Services.Features.Account.Commands.UpdateAddress;
using Services.Features.Account.Commands.UpdatePassword;
using Services.Features.Account.Commands.UpdatePersonalInfo;
using System;

namespace Unit.Tests.Fixtures;

public class AccountCommandFixture
{
    private readonly Faker _faker;

    public AccountCommandFixture()
    {
        _faker = new Faker();
    }

    public UpdateAddressCommand GenerateUpdateAddressCommand()
    {
        var street = _faker.Address.StreetAddress();
        var neighborhood = _faker.Address.County();
        var city = _faker.Address.City();
        var complement = _faker.Lorem.Sentence();

        return new UpdateAddressCommand
        {
            Cep = _faker.Random.Replace("########"), // 8 digits as required by validator
            Street = street.Length > 200 ? street.Substring(0, 200) : street,
            Number = _faker.Random.Number(1, 9999).ToString(),
            Complement = _faker.Random.Bool() && complement.Length > 200 ? complement.Substring(0, 200) : (_faker.Random.Bool() ? complement : null),
            Neighborhood = neighborhood.Length > 90 ? neighborhood.Substring(0, 90) : neighborhood,
            City = city.Length > 90 ? city.Substring(0, 90) : city,
            State = _faker.PickRandom("SP", "RJ", "MG", "RS", "SC", "PR", "BA", "PE") // 2 characters as required
        };
    }

    public UpdatePasswordCommand GenerateUpdatePasswordCommand()
    {
        var password = _faker.Internet.Password(6); // Minimum 6 characters
        var newPassword = _faker.Internet.Password(6); // Minimum 6 characters

        return new UpdatePasswordCommand
        {
            CurrentPassword = password,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        };
    }

    public UpdatePersonalInfoCommand GenerateUpdatePersonalInfoCommand()
    {
        return new UpdatePersonalInfoCommand
        {
            FullName = _faker.Name.FirstName() + " " + _faker.Name.LastName(), // Must contain space
            PhoneNumber = _faker.Random.Replace("(##) #####-####"), // Valid phone format
            CPF_CNPJ = _faker.Random.Replace("###########"), // 11 digits for CPF
            DateOfBirth = _faker.Date.Past(30, DateTime.Now.AddYears(-18))
        };
    }
}
