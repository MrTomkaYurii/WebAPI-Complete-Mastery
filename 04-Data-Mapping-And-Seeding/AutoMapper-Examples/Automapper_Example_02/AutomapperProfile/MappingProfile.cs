using AutoMapper;
using Automapper_Example_02.DTO;
using Automapper_Example_02.Entities;

namespace Automapper_Example_02.AutomapperProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserEntity, UserDTO>();
        }
    }
}
