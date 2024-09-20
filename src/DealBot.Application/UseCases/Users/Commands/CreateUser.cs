namespace DealBot.Application.Users.Commands.CreateUser;

using AutoMapper;
using DealBot.Application.Common;
using DealBot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

public record CreateUserCommand : IRequest<UserResultDto>
{
    public CreateUserCommand(CreateUserCommand command)
    {
        Email = command.Email;
        IsBot = command.IsBot;
        ChatId = command.ChatId;
        FormFile = command.FormFile;
        LastName = command.LastName;
        Password = command.Password;
        Username = command.Username;
        IsPremium = command.IsPremium;
        FirstName = command.FirstName;
        DateOfBirth = command.DateOfBirth;
        LanguageCode = command.LanguageCode;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string LanguageCode { get; set; }
    public long TelegramId { get; set; }
    public bool IsPremium { get; set; }
    public bool IsBot { get; set; }
    public long? ChatId { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public IFormFile? FormFile { get; set; }
}

public class CreateUserCommandHandler(IAppDbContext context, IMapper mapper)
    : IRequestHandler<CreateUserCommand, UserResultDto>
{
    public async Task<UserResultDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(mapper.Map<User>(request), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<UserResultDto>(request);
    }
}