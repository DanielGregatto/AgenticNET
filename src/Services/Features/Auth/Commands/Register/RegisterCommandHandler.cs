using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Model.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Services.Core;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : BaseCommandHandler,
        IRequestHandler<RegisterCommand, Result<RegisterDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IValidator<RegisterCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public RegisterCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IEmailService _emailService,
            IValidator<RegisterCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            this._emailService = _emailService;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<RegisterDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<RegisterCommand, RegisterDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<RegisterDto>.Failure(
                    _localizer["Auth_EmailAlreadyRegistered"],
                    ErrorTypes.Conflict);
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(e.Description, ErrorTypes.Validation)).ToArray();
                return Result<RegisterDto>.ValidationFailure(errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{request.ConfirmationBaseUrl}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

            return Result<RegisterDto>.Success(new RegisterDto(user.Id));
        }
    }
}
