using Domain.Contracts.Common;
using Identity.Model.Responses;
using MediatR;

namespace Services.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<Result<LoginDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
