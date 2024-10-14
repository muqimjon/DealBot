namespace DealBot.Application.UseCases.Users.Queries;

using AutoMapper;
using DealBot.Application.Common;
using DealBot.Application.Users.Commands.CreateUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetUserByTelegramIdQuery(long TelegramId) : IRequest<UserResultDto>;

public class GetUserByTelegramIdQueryHandler(IAppDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetUserByTelegramIdQuery, UserResultDto>
{
    public async Task<UserResultDto> Handle(GetUserByTelegramIdQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.Address)
            .Include(user => user.Contact)
            .Include(user => user.Image)
            .Include(user => user.ReferredBy)
            //.Include(user => user.Store)
            //.Include(user => user.Transactions)
            //.Include(user => user.Reviews)
            .Include(user => user.ReferralsInitiated)
            .FirstOrDefaultAsync(user
                => user.TelegramId.Equals(request.TelegramId), cancellationToken);

        return mapper.Map<UserResultDto>(user);
    }
}
