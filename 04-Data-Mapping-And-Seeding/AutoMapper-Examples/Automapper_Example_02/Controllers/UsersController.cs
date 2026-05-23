using AutoMapper;
using Automapper_Example_02.DTO;
using Automapper_Example_02.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Automapper_Example_02.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        public UsersController(IMapper mapper)
        {
            _mapper = mapper;
        }

        private static readonly List<UserEntity> users = new List<UserEntity>
        {
            new UserEntity
            {
                Id = 1,
                FirstName = "Sam",
                LastName = "Doe",
                Username = "sam_doe",
                Password = "Secure_Password"
            },
            new UserEntity{
                Id = 2,
                FirstName = "John",
                LastName = "Cruise",
                Username = "john-cruise",
                Password = "Password"
            }
        };

        [HttpGet("/get-all")]
        public IEnumerable<UserEntity> GetAll()
        {
            return users;
        }

        [HttpGet("/mapper-get-all")]
        public IEnumerable<UserDTO> MapperGetAll()
        {
            var usersDTO = _mapper.Map<List<UserDTO>>(users);
            return usersDTO;
        }
    }
}
