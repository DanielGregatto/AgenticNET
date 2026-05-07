using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;

namespace Services.Features.Account.Commands.UpdateAddress
{
    public class UpdateAddressCommand : IRequest<Result<ProfileResult>>
    {
        public string Cep { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string? Complement { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
