using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using Identity.Model;
using Identity.Model.Responses;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Services.Core;
using Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : BaseCommandHandler,
        IRequestHandler<RefreshTokenCommand, Result<LoginDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IValidator<RefreshTokenCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public RefreshTokenCommandHandler(
            Data.Context.AppDbContext context,
            IUser user,

            UserManager<ApplicationUser> userManager,
            IRefreshTokenService refreshTokenService,
            IJwtTokenGenerator jwtTokenGenerator,
            IValidator<RefreshTokenCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<LoginDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation using helper
            var validationError = await ValidateAsync<RefreshTokenCommand, LoginDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Result<LoginDto>.ValidationFailure(
                    new Error(_localizer["Auth_RefreshTokenInvalid"], ErrorTypes.Validation));
            }

            var refreshToken = await _refreshTokenService.GetAsync(request.UserId, request.RefreshToken);
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                return Result<LoginDto>.Unauthorized(_localizer["Auth_RefreshTokenExpired"]);
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<LoginDto>.NotFound(_localizer["Auth_UserNotFound"]);
            }

            var newAccessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
            var newRefreshToken = request.RefreshToken;

            // Rotate refresh token if it's expiring soon (less than 5 days)
            if (refreshToken.ExpiryDate < DateTime.UtcNow.AddDays(5))
            {
                newRefreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);
                await _refreshTokenService.RevokeAsync(refreshToken.Token);
            }

            return Result<LoginDto>.Success(new LoginDto(newAccessToken, newRefreshToken));
        }
    }
}
