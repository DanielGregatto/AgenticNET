using Domain.Contracts.Common;
using Identity.Model.Responses;
using MediatR;
using System.Text.Json.Serialization;

namespace Services.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<RegisterDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        [JsonIgnore]
        public string ConfirmationBaseUrl { get; set; }
    }
}
