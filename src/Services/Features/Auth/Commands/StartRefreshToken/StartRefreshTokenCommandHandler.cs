using Domain.Contracts.Common;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Model.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Services.Core;
using Services.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.StartRefreshToken
{
    public class StartRefreshTokenCommandHandler : BaseCommandHandler,
        IRequestHandler<StartRefreshTokenCommand, Result<LoginDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IValidator<StartRefreshTokenCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public StartRefreshTokenCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IValidator<StartRefreshTokenCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<LoginDto>> Handle(StartRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<StartRefreshTokenCommand, LoginDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<LoginDto>.NotFound(_localizer["Auth_UserNotFound"]);
            }

            var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

            return Result<LoginDto>.Success(new LoginDto(accessToken, refreshToken));
        }
    }
}
