using AutoMapper;

using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;

namespace ReferralBot.Core.Mappings;

public class TelegramBotUserProfile : Profile
{
    public TelegramBotUserProfile()
    {
        CreateMap<TelegramBotUserEntity, TelegramBotUser>();
        CreateMap<TelegramBotUser, TelegramBotUserEntity>();
    }
}
