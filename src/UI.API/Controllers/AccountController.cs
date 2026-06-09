using Domain.Contracts.API;
using Services.Contracts.Results;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Features.Account.Commands.UpdateAddress;
using Services.Features.Account.Commands.UpdatePassword;
using Services.Features.Account.Commands.UpdatePersonalInfo;
using Services.Features.Account.Queries.GetUserProfile;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    /// <summary>
    /// Authenticated user account management. View and update profile, address, and password.
    /// All endpoints require a Bearer JWT.
    /// </summary>
    public class AccountController : CoreController
    {
        private readonly IMediatorHandler _mediator;
        private readonly IUser _user;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IMediatorHandler mediator, IUser user, ILogger<AccountController> logger)
        {
            _mediator = mediator;
            _user = user;
            _logger = logger;
        }

        /// <summary>
        /// Get the profile of the currently authenticated user.
        /// </summary>
        [Authorize]
        [HttpGet("v1/account/profile")]
        [ProducesResponseType(typeof(SuccessResponse<ProfileResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Profile()
        {
            var userId = _user.GetUserId();
            _logger.LogInformation("Getting profile for user: {UserId}", userId);

            var result = await _mediator.SendCommand(new GetUserProfileQuery());

            if (result.IsSuccess)
                _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userId);
            else
                _logger.LogWarning("Failed to retrieve profile for user: {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Update the authenticated user's personal details (full name, phone, document number, date of birth).
        /// </summary>
        /// <param name="command">Fields to update.</param>
        [Authorize]
        [HttpPost("v1/account/update-personal-info")]
        [ProducesResponseType(typeof(SuccessResponse<ProfileResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePersonalInfoCommand command)
        {
            var userId = _user.GetUserId();
            _logger.LogInformation("Updating personal info for user: {UserId}", userId);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Personal info updated successfully for user: {UserId}", userId);
            else
                _logger.LogWarning("Failed to update personal info for user: {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Update the authenticated user's address.
        /// </summary>
        /// <param name="command">New address details.</param>
        [Authorize]
        [HttpPost("v1/account/update-address")]
        [ProducesResponseType(typeof(SuccessResponse<ProfileResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressCommand command)
        {
            var userId = _user.GetUserId();
            _logger.LogInformation("Updating address for user: {UserId}", userId);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Address updated successfully for user: {UserId}", userId);
            else
                _logger.LogWarning("Failed to update address for user: {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Change the authenticated user's password. Current password is required for verification.
        /// </summary>
        /// <param name="command">Current password, new password, and confirmation.</param>
        [Authorize]
        [HttpPost("v1/account/update-password")]
        [ProducesResponseType(typeof(SuccessResponse<string>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordCommand command)
        {
            var userId = _user.GetUserId();
            _logger.LogInformation("Updating password for user: {UserId}", userId);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Password updated successfully for user: {UserId}", userId);
            else
                _logger.LogWarning("Failed to update password for user: {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }
    }
}