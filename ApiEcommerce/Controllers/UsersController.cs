using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [ApiVersionNeutral]

    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository= userRepository;
            //_categoryRepository= categoryRepository;
            _mapper =mapper;

        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            var userDto = _mapper.Map<List<UserDto>>(users);
            return Ok(userDto);
        }
        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUser(string id)
        {
            var user = _userRepository.GetUser(id);

            if (user == null)
            {
                return NotFound($"El usuario con el id {id} no existe.");
            }

            var usersDto = _mapper.Map<UserDto>(user);

            return Ok(usersDto);
        }

        [AllowAnonymous]
        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if(createUserDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(string.IsNullOrWhiteSpace(createUserDto.Username))
            {
                return BadRequest("Username es requerido");
            }
            if(!_userRepository.IsUniqueUser(createUserDto.Username))
            {
                return BadRequest("El usario ya existe");
            }

            var result = await _userRepository.Register(createUserDto);
            if(result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"Error al registrar el usuario");
            }
            return CreatedAtRoute("GetUser",new{ id = result.Id },result);
        }
        [AllowAnonymous]
        [HttpPost("Login",Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] UserLoginDto userLoginDto)
        {
            if(userLoginDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            var user = await _userRepository.Login(userLoginDto);
            if(user == null)
            {
                return Unauthorized();
            }
            return Ok(user);
        }

        
    }
}