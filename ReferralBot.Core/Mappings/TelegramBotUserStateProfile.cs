using AutoMapper;

using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;

namespace ReferralBot.Core.Mappings;

public class TelegramBotUserStateProfile : Profile
{
    public TelegramBotUserStateProfile()
    {
        CreateMap<TelegramBotUserStateEntity, TelegramBotUserState>();
        CreateMap<TelegramBotUserState, TelegramBotUserStateEntity>();
    }
}
