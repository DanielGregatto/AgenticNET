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
        /// Retrieves the profile information for the currently authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires authentication. If the user is not authenticated, the request
        /// will be rejected with a 401 Unauthorized response.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the user's profile data if
        /// found; otherwise, an <see cref="ErrorResponseDto"/> indicating the error.</returns>
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
        /// Updates the authenticated user's personal information.
        /// </summary>
        /// <remarks>This endpoint allows an authenticated user to update their personal details, such as
        /// full name, phone number, identification number, and date of birth. The user must be authenticated to access
        /// this endpoint.</remarks>
        /// <param name="command">The command containing the updated personal information for the user.</param>
        /// <returns>A <see cref="SuccessResponse{T}"/> containing the updated profile information if the update is successful;
        /// otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
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
        /// Updates the address information for the authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires authentication. The address is updated for the currently
        /// authenticated user only. If the user is not found or authentication fails, an error response is
        /// returned.</remarks>
        /// <param name="command">The command containing the new address details to be applied to the user's profile.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the updated <see
        /// cref="ProfileResult"/> if the operation succeeds; otherwise, an <see cref="ErrorResponseDto"/> describing the
        /// error.</returns>
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
        /// Updates the password for the currently authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires authentication. The user must provide their current password
        /// and a new password. The new password must meet any password policy requirements enforced by the system. If
        /// the update is successful, a success response is returned; otherwise, an error response is provided with
        /// details.</remarks>
        /// <param name="command">The command containing the current password, new password, and confirmation of the new password.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with a success message if the
        /// password is updated successfully; otherwise, an <see cref="ErrorResponseDto"/> with error details.</returns>
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