namespace DealBot.Application.UseCases.Users.Mappers;

using AutoMapper;
using DealBot.Application.Users.Commands.CreateUser;
using DealBot.Domain.Entities;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResultDto>();
        CreateMap<CreateUserCommand, User>();
        CreateMap<UpdateUserCommand, User>();
        CreateMap<CreateUserCommand, UserResultDto>();
        CreateMap<UpdateUserCommand, UserResultDto>();
    }
}