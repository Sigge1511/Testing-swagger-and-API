using apiv4.Constants;
using apiv4.Dto;
using apiv4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace apiv4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApiUser> _signInManager;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _config = configuration;
        }
        
        //***************************************************************
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {

            try
            {
                ApiUser user = new()
                {
                    UserName = userDto.Email,
                    Email = userDto.Email,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                };
                var result = await _userManager.CreateAsync(user, userDto.Password); //UserManager ser till att ett Hashat Password skickas med/in. 

                if (result.Succeeded == false)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                await _userManager.AddToRoleAsync(user, Constants.ApiRole.User);
                return Accepted();
            }
            catch (Exception ex)
            {
                return Problem($"Something went wrong in the {nameof(Register)}", statusCode: 500);

            }
            //ska inte skapa token - user ska bara sparas i db här. Logga in i eget steg sen
        }

        //***************************************************************
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginUserDto userDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userDto.Email);
                var passwordValid = await _userManager.CheckPasswordAsync(user, userDto.Password); // Bool som jämför password från user och det som skickas in userDto.Password dvs

                if (user == null || passwordValid == false) // För att inte ge ut om bara password eller User är felaktigt. 
                {
                    return NotFound(); // Skickas              
                }

                //Skapa Token
                string tokenString = await GenerateToken(user);
                var response = new AuthResponseDto
                {
                    UserId = user.Id,
                    Token = tokenString,
                    Email = userDto.Email
                };
                //var result = await _signInManager.PasswordSignInAsync(user, userDto.Password, isPersistent: true, lockoutOnFailure: false);

                //if (!result.Succeeded) return Unauthorized();

                return Ok(response);
            }
            catch (Exception)
            {
                return Problem($"Something went wrong in the {nameof(Login)}", statusCode: 500);
            }
        }

        private async Task<string> GenerateToken(ApiUser user)
        {
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(user);


            var roleClaims = roles.Select(q => new Claim(ClaimTypes.Role, q)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(CustomClaimTypes.Uid, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)

            }.Union(roleClaims)
             .Union(userClaims);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_config["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //***************************************************************
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // tar bort cookie
            return Ok(new { message = "Logged out" });
        }

    }
}
