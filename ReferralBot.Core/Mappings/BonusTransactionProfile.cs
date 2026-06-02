using AutoMapper;

using CoreModels = ReferralBot.Core.Models;
using DbEntities = ReferralBot.Db.Entities;

namespace ReferralBot.Core.Mappings;

public class BonusTransactionProfile : Profile
{
    public BonusTransactionProfile()
    {
        CreateMap<DbEntities.BonusTransactionEntity, CoreModels.BonusTransaction>()
            .ForMember(
                dest => dest.OperationType,
                opt => opt.MapFrom(src => (CoreModels.BonusOperationType)src.OperationType));

        CreateMap<CoreModels.BonusTransaction, DbEntities.BonusTransactionEntity>()
            .ForMember(
                dest => dest.OperationType,
                opt => opt.MapFrom(src => (DbEntities.BonusOperationType)src.OperationType));
    }
}
