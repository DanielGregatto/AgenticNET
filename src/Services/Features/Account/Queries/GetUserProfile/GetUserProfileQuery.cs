using Domain.Contracts.Common;
using Services.Contracts.Results;
using MediatR;

namespace Services.Features.Account.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<Result<ProfileResult>>
    {
    }
}
