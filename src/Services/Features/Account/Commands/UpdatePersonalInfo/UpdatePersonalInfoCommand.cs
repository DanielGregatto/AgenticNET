using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;
using System;

namespace Services.Features.Account.Commands.UpdatePersonalInfo
{
    public class UpdatePersonalInfoCommand : IRequest<Result<ProfileResult>>
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CPF_CNPJ { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
