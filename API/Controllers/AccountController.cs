using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Dtos;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(IUserServices userServices) : ControllerBase
    {
        private readonly IUserServices _userServices = userServices;

        [HttpPost("register")]
        public async Task<ActionResult<CommonResponse<dynamic>>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _userServices.RegisterUser(registerDto);
            return Ok(result);

        }

        [HttpPost("login")]
        public async Task<ActionResult<CommonResponse<dynamic>>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userServices.LoginUser(loginDto);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("validate-token")]
        public async Task<ActionResult<CommonResponse<dynamic>>> ValidateToken([FromBody]string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var result = await _userServices.ValidateToken(token);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}