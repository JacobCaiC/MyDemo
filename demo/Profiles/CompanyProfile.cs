using AutoMapper;
using MyDemo.Entities;
using MyDemo.Models.Dto;

namespace MyDemo.Profiles
{
    /// <summary>
    /// AutoMapper 针对 Company 的映射关系配置文件（P12）
    /// </summary>
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            //原类型Company -> 目标类型CompanyDto AutoMapper 基于约定属性名称一致时自动赋值,自动忽略空引用
            //CreateMap<Company, CompanyDto>()；

            //自定义属性映射
            CreateMap<Company, CompanyDto>()
                 .ForMember(
                    destinationMember: dest => dest.CompanyName,
                    memberOptions: opt => opt.MapFrom(mapExpression: src => src.Name));

            CreateMap<CompanyAddDto, Company>();
            CreateMap<Company, CompanyFullDto>();
            CreateMap<CompanyAddWithBankruptTimeDto, Company>();
        }
    }
}
