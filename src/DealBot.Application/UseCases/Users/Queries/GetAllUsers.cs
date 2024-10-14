namespace DealBot.Application.UseCases.Users.Queries;

using AutoMapper;
using DealBot.Application.Common;
using DealBot.Application.Users.Commands.CreateUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllUsersQuery : IRequest<IEnumerable<UserResultDto>>;

public class GetAllUsersQueryHandler(IAppDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetAllUsersQuery, IEnumerable<UserResultDto>>
{
    public async Task<IEnumerable<UserResultDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users.Include(user => user.Address)
            .Include(user => user.Contact)
            .Include(user => user.Image)
            .Include(user => user.ReferredBy)
            //.Include(user => user.Store)
            //.Include(user => user.Reviews)
            .Include(user => user.ReferralsInitiated)
            .ToListAsync(cancellationToken: cancellationToken);

        return mapper.Map<IEnumerable<UserResultDto>>(users);
    }
}
