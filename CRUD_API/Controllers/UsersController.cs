﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Company_API.Services;
using Company_API.Models.DTO;
using Company_API.Models;

namespace Company_API.Controllers 
{
    [ApiController]
    [Route("api/Users")]
    public class UsersController : ControllerBase  
    {
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public UsersController(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] LoginDTO model)
        {
            var user = _userService.Authenticate(model);

            if (user == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            UserRole userRole = _userService.GetUserRole(user.UserRoleId);

            var token = GenerateJwtToken(user.Username, userRole.Name);
            var refreshToken = GenerateRefreshToken();

            await _userService.SaveRefreshToken(user.Id, refreshToken);

            return new
            {
                Token = token,
                RefreshToken = refreshToken
            };
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<dynamic>> Refresh([FromBody] RefreshTokenDTO model)
        {
            var user = _userService.GetUserByRefreshToken(model.RefreshToken);
            UserRole userRole = _userService.GetUserRole(user.UserRoleId);
            var token = GenerateJwtToken(user.Username, userRole.Name);
            var newRefreshToken = GenerateRefreshToken();

            await _userService.SaveRefreshToken(user.Id, newRefreshToken);

            return new
            {
                Token = token,
                RefreshToken = newRefreshToken
            };
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterDTO registerDto)
        {
            try
            {
                var user = await _userService.Register(registerDto);
                UserRole userRole = _userService.GetUserRole(user.UserRoleId);
                var token = GenerateJwtToken(user.Username, userRole.Name);
                var refreshToken = GenerateRefreshToken();

                await _userService.SaveRefreshToken(user.Id, refreshToken);

                var userDto = new UserDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    RefreshToken = refreshToken
                };
                return Ok(userDto);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateJwtToken(string username, string role)
        {
            var claims = new[] 
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role) // or get roles from database or other data source
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

}
