using Domain.Contracts.Common;
using Identity.Model.Responses;
using MediatR;

namespace Services.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<Result<ForgotPasswordDto>>
    {
        public string Email { get; set; }
    }
}
