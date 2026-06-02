using AutoMapper;

using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;

namespace ReferralBot.Core.Mappings;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<AccountEntity, Account>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (UserStatus)src.Status));

        CreateMap<Account, AccountEntity>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (UserDbStatus)src.Status));
    }
}
