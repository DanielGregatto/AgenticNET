using Bogus;
using Identity.Model;
using System;
using System.Collections.Generic;

namespace Unit.Tests.Fixtures;

public class ApplicationUserFixture
{
    private readonly Faker<ApplicationUser> _userFaker;

    public ApplicationUserFixture()
    {
        _userFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.CPF_CNPJ, f => f.Random.Replace("###########"))
            .RuleFor(u => u.DateOfBirth, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.Street, f => f.Address.StreetAddress())
            .RuleFor(u => u.Number, f => f.Random.Number(1, 9999).ToString())
            .RuleFor(u => u.Complement, f => f.Random.Bool() ? f.Address.SecondaryAddress() : null)
            .RuleFor(u => u.Neighborhood, f => f.Address.County())
            .RuleFor(u => u.City, f => f.Address.City())
            .RuleFor(u => u.State, f => f.Address.StateAbbr())
            .RuleFor(u => u.ZipCode, f => f.Address.ZipCode())
            .RuleFor(u => u.Country, f => f.Address.Country());
    }

    public ApplicationUser GenerateUser()
    {
        return _userFaker.Generate();
    }

    public ApplicationUser GenerateConfirmedUser()
    {
        var user = _userFaker.Generate();
        user.EmailConfirmed = true;
        return user;
    }

    public ApplicationUser GenerateUnconfirmedUser()
    {
        var user = _userFaker.Generate();
        user.EmailConfirmed = false;
        return user;
    }

    public List<ApplicationUser> GenerateUserList(int count)
    {
        return _userFaker.Generate(count);
    }
}
