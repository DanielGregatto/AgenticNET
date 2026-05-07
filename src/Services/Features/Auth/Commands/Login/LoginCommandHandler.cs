using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Model.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using Services.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : BaseCommandHandler,
        IRequestHandler<LoginCommand, Result<LoginDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IValidator<LoginCommand> _validator;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public LoginCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IValidator<LoginCommand> validator,
            ILogger<LoginCommandHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing LoginCommand for email: {Email}", request.Email);

            var validationError = await ValidateAsync<LoginCommand, LoginDto>(_validator, request, cancellationToken);
            if (validationError != null)
            {
                _logger.LogWarning("Login validation failed for email: {Email}", request.Email);
                return validationError;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Login failed - invalid credentials for email: {Email}", request.Email);
                return Result<LoginDto>.Unauthorized(_localizer["Auth_InvalidCredentials"]);
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login failed - email not confirmed for user: {Email}", request.Email);
                return Result<LoginDto>.ValidationFailure(
                    new Error(_localizer["Auth_EmailNotConfirmedBeforeLogin"], ErrorTypes.Validation));
            }

            var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

            _logger.LogInformation("Login successful for user: {Email}, UserId: {UserId}", request.Email, user.Id);

            var response = new LoginDto(accessToken, refreshToken);
            return Result<LoginDto>.Success(response);
        }
    }
}
