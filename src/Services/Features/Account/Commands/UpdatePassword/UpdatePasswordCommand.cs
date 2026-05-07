using Domain.Contracts.Common;
using MediatR;

namespace Services.Features.Account.Commands.UpdatePassword
{
    public class UpdatePasswordCommand : IRequest<Result<string>>
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
