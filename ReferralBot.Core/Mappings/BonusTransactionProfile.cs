using AutoMapper;

using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;

namespace ReferralBot.Core.Mappings;

public class BonusTransactionProfile : Profile
{
    public BonusTransactionProfile()
    {
        CreateMap<BonusTransactionEntity, BonusTransaction>()
            .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => (BonusOperationType)src.OperationType));

        CreateMap<BonusTransaction, BonusTransactionEntity>()
            .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => (Db.Entities.BonusOperationType)src.OperationType));
    }
}
