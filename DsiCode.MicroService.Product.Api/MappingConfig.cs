using AutoMapper;
using DsiCode.MicroService.Product.API.Models.Dto;

namespace DsiCode.MicroService.Product.API
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<ProductDto, DsiCode.MicroService.Product.API.Models.Product>()
                .ReverseMap();
            });
            return mappingConfig;
        }
    }
}
