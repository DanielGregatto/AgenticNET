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
        /// Authenticates a user with the provided credentials and returns a login response.
        /// </summary>
        /// <remarks>This endpoint expects a POST request with user credentials in the request body. If
        /// the credentials are valid, a successful response with login information is returned; otherwise, an error
        /// response is provided.</remarks>
        /// <param name="command">The login command containing the user's email and password.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with login details if
        /// authentication is successful, or an <see cref="ErrorResponseDto"/> if authentication fails.</returns>
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
        /// Registers a new user account using the provided registration details.
        /// </summary>
        /// <remarks>This endpoint creates a new user account and initiates the email confirmation
        /// process.  The client must provide all required registration fields in the request body.  If registration is
        /// successful, an email confirmation link is generated and sent to the user.</remarks>
        /// <param name="command">The registration command containing user information such as email, password, and confirmation password.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the registered user
        /// details if successful;  otherwise, a response indicating the reason for failure.</returns>
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
        /// Resets a user's password using the provided reset token and new password information.
        /// </summary>
        /// <remarks>This endpoint is typically used as part of a password recovery workflow. The reset
        /// token must be valid and not expired. The new password and confirmation must match and meet any password
        /// policy requirements.</remarks>
        /// <param name="command">The command containing the user's email address, reset token, new password, and password confirmation.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with password reset details if
        /// successful, or an <see cref="ErrorResponseDto"/> with error information if the operation fails.</returns>
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
        /// Initiates the password reset process for a user by sending a password reset email to the specified address.
        /// </summary>
        /// <remarks>This endpoint does not reveal whether the email address exists in the system to
        /// prevent user enumeration.  The response will indicate success if the password reset process is initiated, or
        /// an error if the user is not found.</remarks>
        /// <param name="command">The command containing the email address of the user who has forgotten their password.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with password reset details if
        /// the email is found;  otherwise, an <see cref="ErrorResponseDto"/> indicating that the user was not found.</returns>
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
        /// Refreshes the access token using a valid refresh token.
        /// </summary>
        /// <remarks>This endpoint is used to obtain a new access token when the current one has
        /// expired, provided a valid refresh token is supplied. The response will indicate success or provide error
        /// details if the refresh token is invalid or expired.</remarks>
        /// <param name="command">The command containing the user identifier and refresh token to be validated and exchanged for a new access
        /// token.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the new access token and
        /// related login information if the refresh is successful; otherwise, an <see cref="ErrorResponseDto"/>
        /// describing the failure.</returns>
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
        /// Initiates the refresh token process for the currently authenticated user.
        /// </summary>
        /// <remarks>This endpoint is accessible only to authenticated users. It starts the process of
        /// issuing a new refresh token and access token for the user associated with the current authentication
        /// context.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with a <see cref="LoginDto"/>
        /// if the refresh process succeeds; otherwise, an <see cref="ErrorResponseDto"/> with error details and a 401
        /// status code if the operation fails.</returns>
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
        /// Confirms a user's email address using the specified confirmation token.
        /// </summary>
        /// <remarks>This endpoint is typically called when a user clicks the email confirmation link sent
        /// to their email address during registration or email change processes.</remarks>
        /// <param name="email">The email address of the user to confirm. Cannot be null or empty.</param>
        /// <param name="token">The email confirmation token associated with the user. Cannot be null or empty.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to a confirmation page if the email is successfully confirmed;
        /// otherwise, a response indicating the error.</returns>
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
        /// Initiates the Google authentication process by redirecting the user to the Google login page.
        /// </summary>
        /// <remarks>This endpoint starts an OAuth 2.0 authentication flow with Google. Upon successful
        /// authentication, the user is redirected back to the application via the configured callback
        /// endpoint.</remarks>
        /// <returns>A <see cref="ChallengeResult"/> that redirects the user to Google's authentication page.</returns>
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
        /// Initiates the Facebook OAuth login process by redirecting the user to Facebook for authentication.
        /// </summary>
        /// <remarks>This endpoint starts the external login flow using Facebook as the authentication
        /// provider. Upon successful authentication, the user is redirected back to the application via the configured
        /// callback endpoint.</remarks>
        /// <returns>A challenge result that redirects the user to Facebook's login page. The response will be a <see
        /// cref="SuccessResponse{T}"/> containing a <see cref="ChallengeResult"/> if the operation is successful.</returns>
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
        /// Handles the callback from an external authentication provider and completes the external login process.
        /// </summary>
        /// <remarks>This endpoint is invoked by external authentication providers after the
        /// user has completed authentication. It processes the authentication result, issues a token if successful, and
        /// redirects the user accordingly.</remarks>
        /// <param name="returnUrl">The URL to redirect to after successful authentication. If <see langword="null"/>, a default redirect URI is
        /// used.</param>
        /// <param name="remoteError">An error message returned by the external provider, if any. If not <see langword="null"/>, the login process
        /// is halted and an error response is returned.</param>
        /// <param name="provider">The name of the external authentication provider (e.g., "Google", "Facebook").</param>
        /// <returns>An <see cref="IActionResult"/> that redirects the user to the specified return URL with an authentication
        /// token on success, or returns an error response if authentication fails or an error is reported by the
        /// external provider.</returns>
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