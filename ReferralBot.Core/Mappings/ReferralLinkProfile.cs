using AutoMapper;

using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;

namespace ReferralBot.Core.Mappings;

public class ReferralLinkProfile : Profile
{
    public ReferralLinkProfile()
    {
        CreateMap<ReferralLinkEntity, ReferralLink>();
        CreateMap<ReferralLink, ReferralLinkEntity>();
    }
}
