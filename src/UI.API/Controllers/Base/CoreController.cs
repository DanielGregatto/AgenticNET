using Domain.Contracts.API;
using Domain.Contracts.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace UI.API.Controllers.Base
{
    /// <summary>
    /// Base controller for CQRS-based endpoints using Result pattern
    /// </summary>
    [ApiController]
    [Route("api/")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 409)]
    [ProducesResponseType(typeof(ErrorResponseDto), 500)]
    [ProducesResponseType(typeof(ErrorResponseDto), 502)]
    public abstract class CoreController : ControllerBase
    {
        /// <summary>
        /// Helper method to convert Result<T> to IActionResult
        /// </summary>
        protected IActionResult Response<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data
                });
            }

            return HandleError(result.Errors);
        }

        /// <summary>
        /// Helper method to convert Result to IActionResult (non-generic)
        /// </summary>
        protected IActionResult Response(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true
                });
            }

            return HandleError(result.Errors);
        }

        /// <summary>
        /// Maps Result errors to appropriate HTTP status codes
        /// </summary>
        private IActionResult HandleError(List<Error> errors)
        {
            if (errors == null || !errors.Any())
            {
                return BadRequest(CreateErrorResponse(400, "Unknown error", errors));
            }

            var primaryError = errors.First();

            return
                primaryError.Type switch
                {
                    ErrorTypes.Validation => BadRequest(CreateErrorResponse(400, "Validation error", errors)),
                    ErrorTypes.NotFound => NotFound(CreateErrorResponse(404, "Resource not found", errors)),
                    ErrorTypes.Unauthorized => Unauthorized(CreateErrorResponse(401, "Unauthorized", errors)),
                    ErrorTypes.Forbidden => StatusCode(403, CreateErrorResponse(403, "Forbidden", errors)),
                    ErrorTypes.Conflict => Conflict(CreateErrorResponse(409, "Conflict", errors)),
                    ErrorTypes.Database => StatusCode(500, CreateErrorResponse(500, "Database error", errors)),
                    ErrorTypes.External => StatusCode(502, CreateErrorResponse(502, "External service error", errors)),
                    _ => StatusCode(500, CreateErrorResponse(500, "Internal server error", errors))
                };
        }

        /// <summary>
        /// Creates standardized error response
        /// </summary>
        private ErrorResponseDto CreateErrorResponse(int status, string title, List<Error> errors)
        {
            return new ErrorResponseDto(
                status: status,
                title: title,
                traceId: Guid.NewGuid().ToString(),
                type: $"https://tools.ietf.org/html/rfc7231#section-6.5.{status / 100}",
                items: errors?.Select(e => new ErrorResponseItemDto(e.Code, e.Message)).ToList() ?? new List<ErrorResponseItemDto>()
            );
        }
    }
}
