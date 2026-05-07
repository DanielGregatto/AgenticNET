using Domain.Contracts.Common;
using Services.Contracts.Results;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Account.Commands.UpdatePersonalInfo
{
    public class UpdatePersonalInfoCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdatePersonalInfoCommand, Result<ProfileResult>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UpdatePersonalInfoCommandHandler> _logger;
        private readonly IValidator<UpdatePersonalInfoCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public UpdatePersonalInfoCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            ILogger<UpdatePersonalInfoCommandHandler> logger,
            IValidator<UpdatePersonalInfoCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _logger = logger;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<ProfileResult>> Handle(UpdatePersonalInfoCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<UpdatePersonalInfoCommand, ProfileResult>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var user = await _userManager.FindByIdAsync(UserID.ToString());

            if (user == null)
                return Result<ProfileResult>.NotFound(_localizer["Account_UserNotFound"]);

            // Update user properties
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.CPF_CNPJ = request.CPF_CNPJ;
            user.DateOfBirth = request.DateOfBirth;

            // Update user in database
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user personal info for user {UserId}", UserID);
                var errors = result.Errors.Select(e => new Error(e.Description, ErrorTypes.Database)).ToArray();
                return Result<ProfileResult>.ValidationFailure(errors);
            }

            var profile = new ProfileResult
            {
                Id = UserID,
                Email = user.Email ?? "",
                FullName = user.FullName,
                CPF_CNPJ = user.CPF_CNPJ,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Street = user.Street,
                Number = user.Number,
                Complement = user.Complement,
                Neighborhood = user.Neighborhood,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Country = user.Country
            };

            return Result<ProfileResult>.Success(profile);
        }
    }
}
