using AutoMapper;
using Company_API.Models;
using Company_API.Models.DTO;

namespace Company_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Product, ProductDTO>();
            CreateMap<ProductDTO,Product>();

            CreateMap<Product, ProductCreateDTO>().ReverseMap();
            CreateMap<Product, ProductUpdateDTO>().ReverseMap();


            CreateMap<Company, CompanyDTO>().ReverseMap();
            CreateMap<Company, CompanyCreateDTO>().ReverseMap();
            CreateMap<Company, CompanyCreateDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();
        }
    }
}