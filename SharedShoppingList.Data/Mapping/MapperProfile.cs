using AutoMapper;
using SharedShoppingList.Data.Dto;
using SharedShoppingList.Data.Entities;

namespace SharedShoppingList.Data.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<UserShoppingList, MemberDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.IsOwner, opt => opt.MapFrom(src => src.ShoppingList.CreatedByUser == src.User));
            CreateMap<ShoppingList, ShoppingListDto>()
                .ForMember(dest => dest.NumberOfProducts, opt => opt.MapFrom(src => src.Products.Count))
                .ForMember(dest => dest.IsShared, opt => opt.MapFrom(src => src.Users.Count > 1));
        }
    }
}