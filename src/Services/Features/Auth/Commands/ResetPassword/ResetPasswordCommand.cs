using Domain.Contracts.Common;
using Identity.Model.Responses;
using MediatR;

namespace Services.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Result<ResetPasswordDto>>
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
