using Domain.Contracts.API;
using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using Identity.Model;
using Identity.Model.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.Features.Auth.Commands.ConfirmEmail;
using Services.Features.Auth.Commands.ExternalLoginCallback;
using Services.Features.Auth.Commands.ForgotPassword;
using Services.Features.Auth.Commands.Login;
using Services.Features.Auth.Commands.RefreshToken;
using Services.Features.Auth.Commands.Register;
using Services.Features.Auth.Commands.ResetPassword;
using Services.Features.Auth.Commands.StartRefreshToken;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    /// <summary>
    /// Authentication and token management. Obtain a JWT via login, register a new account,
    /// reset a forgotten password, refresh tokens, or sign in with Google or Facebook.
    /// No Bearer token is required except for <c>POST /api/v1/auth/start-refresh</c>.
    /// </summary>
    public class AuthController : CoreController
    {
        private readonly IMediatorHandler _mediator;
        private readonly IUser _user;
        private readonly JWTConfig _jwtConfig;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
                IOptions<JWTConfig> jwtConfig,
                IMediatorHandler mediator,
                IUser user,
                ILogger<AuthController> logger)
        {
            this._jwtConfig = jwtConfig.Value;
            _mediator = mediator;
            _user = user;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate with email and password and receive a JWT access token and refresh token.
        /// </summary>
        /// <param name="command">Email and password credentials.</param>
        [HttpPost("v1/auth/login")]
        [ProducesResponseType(typeof(SuccessResponse<LoginDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            _logger.LogInformation("Login attempt for user: {Email}", command.Email);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("User logged in successfully: {Email}", command.Email);
            else
                _logger.LogWarning("Login failed for user: {Email}. Errors: {Errors}",
                    command.Email, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Create a new user account and trigger an email confirmation link.
        /// </summary>
        /// <param name="command">Email, password, and confirmation password for the new account.</param>
        [HttpPost("v1/auth/register")]
        [ProducesResponseType(typeof(SuccessResponse<RegisterDto>), 200)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            _logger.LogInformation("Registration attempt for user: {Email}", command.Email);

            command.ConfirmationBaseUrl = Url.Action(nameof(ConfirmEmail), "Auth", new { }, Request.Scheme);
            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("User registered successfully: {Email}", command.Email);
            else
                _logger.LogWarning("Registration failed for user: {Email}. Errors: {Errors}",
                    command.Email, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }


        /// <summary>
        /// Set a new password using the reset token received by email.
        /// </summary>
        /// <remarks>The reset token is delivered via the forgot-password flow and expires after a short window.</remarks>
        /// <param name="command">Email address, the reset token from the email link, and the new password.</param>
        [HttpPost("v1/auth/reset-password")]
        [ProducesResponseType(typeof(SuccessResponse<ResetPasswordDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            _logger.LogInformation("Password reset attempt for user: {Email}", command.Email);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Password reset successfully for user: {Email}", command.Email);
            else
                _logger.LogWarning("Password reset failed for user: {Email}. Errors: {Errors}",
                    command.Email, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Request a password reset link to be sent to the given email address.
        /// </summary>
        /// <param name="command">The email address of the account to recover.</param>
        [HttpPost("v1/auth/forgot-password")]
        [ProducesResponseType(typeof(SuccessResponse<ForgotPasswordDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            _logger.LogInformation("Forgot password request for user: {Email}", command.Email);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Forgot password email sent for user: {Email}", command.Email);
            else
                _logger.LogWarning("Forgot password failed for user: {Email}. Errors: {Errors}",
                    command.Email, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Exchange a valid refresh token for a new access token and refresh token pair.
        /// </summary>
        /// <param name="command">User ID and the current refresh token.</param>
        [HttpPost("v1/auth/refresh")]
        [ProducesResponseType(typeof(SuccessResponse<LoginDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        {
            _logger.LogInformation("Token refresh attempt for user: {UserId}", command.UserId);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Token refreshed successfully for user: {UserId}", command.UserId);
            else
                _logger.LogWarning("Token refresh failed for user: {UserId}. Errors: {Errors}",
                    command.UserId, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Issue a new refresh token for the currently authenticated user. Requires a valid Bearer JWT.
        /// </summary>
        [Authorize]
        [HttpPost("v1/auth/start-refresh")]
        [ProducesResponseType(typeof(SuccessResponse<LoginDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        public async Task<IActionResult> StartRefresh()
        {
            var userId = _user.GetUserId();
            _logger.LogInformation("Start refresh token for user: {UserId}", userId);

            var command = new StartRefreshTokenCommand
            {
                UserId = userId
            };
            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Refresh token started successfully for user: {UserId}", userId);
            else
                _logger.LogWarning("Start refresh token failed for user: {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Confirm a user's email address. Called automatically when the user clicks the link in the confirmation email.
        /// </summary>
        /// <param name="email">The email address being confirmed.</param>
        /// <param name="token">The one-time confirmation token from the email link.</param>
        [HttpGet("v1/auth/email-confirmed")]
        [ProducesResponseType(typeof(SuccessResponse<UriBuilder>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            _logger.LogInformation("Email confirmation attempt for user: {Email}", email);

            var command = new ConfirmEmailCommand
            {
                Email = email,
                Token = token
            };
            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("Email confirmed successfully for user: {Email}", email);
                return Redirect(result.Data.ToString());
            }

            _logger.LogWarning("Email confirmation failed for user: {Email}", email);
            return Response(result);
        }

        /// <summary>
        /// Redirect the browser to Google's OAuth 2.0 consent screen to begin social sign-in.
        /// </summary>
        [HttpGet("v1/auth/google-login")]
        [ProducesResponseType(typeof(ChallengeResult), 200)]
        public IActionResult GoogleLogin()
        {
            _logger.LogInformation("Google login initiated");

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { provider = GoogleDefaults.AuthenticationScheme }, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        /// <summary>
        /// Redirect the browser to Facebook's OAuth consent screen to begin social sign-in.
        /// </summary>
        [HttpGet("v1/auth/facebook-login")]
        [ProducesResponseType(typeof(SuccessResponse<ChallengeResult>), 200)]
        public IActionResult FacebookLogin()
        {
            _logger.LogInformation("Facebook login initiated");

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { provider = FacebookDefaults.AuthenticationScheme }, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// OAuth callback invoked by Google or Facebook after the user approves the consent screen.
        /// Issues a JWT and redirects to the configured frontend URI with the token in the query string.
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after authentication.</param>
        /// <param name="remoteError">Error message from the external provider, if any.</param>
        /// <param name="provider">Name of the external provider (e.g. "Google", "Facebook").</param>
        [AllowAnonymous]
        [HttpGet("v1/auth/external-login-callback")]
        [ProducesResponseType(typeof(SuccessResponse<RedirectResult>), 200)]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null,
                                                               string? remoteError = null,
                                                               string? provider = null)
        {
            _logger.LogInformation("External login callback from provider: {Provider}", provider);

            if (remoteError != null)
            {
                _logger.LogWarning("External login error from {Provider}: {Error}", provider, remoteError);
                var errorResult = Result<string>.Failure(
                    $"Erro externo: {remoteError}",
                    ErrorTypes.Validation);
                return Response(errorResult);
            }

            var authResult = await HttpContext.AuthenticateAsync(provider);
            var command = new ExternalLoginCallbackCommand
            {
                Provider = provider,
                AuthenticateResult = authResult
            };
            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("External login successful for provider: {Provider}", provider);
                var redirectUri = $"{_jwtConfig.RedirectUriExternalLogin}?token={Uri.EscapeDataString(result.Data)}";
                return Redirect(redirectUri);
            }

            _logger.LogWarning("External login failed for provider: {Provider}. Errors: {Errors}",
                provider, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }
    }
}