using AutoMapper;
using GreenSpace.Application.DTOs.Payment;
using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Mappings.AutoMappers
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(d => d.BankCode, o => o.Ignore())
                .ForMember(d => d.BankTranNo, o => o.Ignore())
                .ForMember(d => d.CardType, o => o.Ignore());
        }
    }
}