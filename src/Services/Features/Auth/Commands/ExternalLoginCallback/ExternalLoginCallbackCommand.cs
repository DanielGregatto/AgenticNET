using Domain.Contracts.Common;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;

namespace Services.Features.Auth.Commands.ExternalLoginCallback
{
    public class ExternalLoginCallbackCommand : IRequest<Result<string>>
    {
        public string Provider { get; set; }
        public AuthenticateResult AuthenticateResult { get; set; }
    }
}
