using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Dtos;
using API.Jwt;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public interface IUserServices
    {
        Task<CommonResponse<AuthResponseDto>> RegisterUser(RegisterDto request);
        Task<CommonResponse<AuthResponseDto>> LoginUser(LoginDto request);
        Task<CommonResponse<ValidateTokenResponse>> ValidateToken(string token);
    }
    public class UserServices(IConfiguration configuration, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager) : IUserServices
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;

        public async Task<CommonResponse<AuthResponseDto>> RegisterUser(RegisterDto request)
        {
            var response = new CommonResponse<AuthResponseDto>();
            var user = new AppUser
            {
                Email = request.Email,
                FullName = request.FullName,
                UserName = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                response.Success = false;
                response.Error = new ErrorItem
                {
                    Message = "User Registration Failed",
                    Exception = new Exception(result.Errors.FirstOrDefault()?.Description)
                };

                return response;
            }

            if (request.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in request.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            response.Success = true;
            return response;
        }

        public async Task<CommonResponse<AuthResponseDto>> LoginUser(LoginDto request)
        {
            var response = new CommonResponse<AuthResponseDto>();
            var user = await _userManager.FindByEmailAsync(request.Email ?? "");
            if (user is null)
            {
                response.Success = false;
                response.Error = new ErrorItem
                {
                    Message = "User not found",
                };
                return response;
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password ?? "");

            if (!result)
            {
                response.Success = false;
                response.Error = new ErrorItem
                {
                    Message = "Invalid Password",
                };
                return response;
            }

            var token = GenerateToken(user);

            response.Success = true;
            response.Payload = new AuthResponseDto
            {
                Token = token,
                Message = "Login Successful"
            };
            return response;
        }
        private string GenerateToken(AppUser user)
        {
            // var claims = new List<Claim>
            // {
            //     new("UserId", user.Id.ToString()),
            //     new("Email", user.Email ?? ""),
            //     new("JwtRegisteredClaimNames.Jti", Guid.NewGuid().ToString())
            // };
            // var roles = _userManager.GetRolesAsync(user).Result;
            // foreach (var role in roles)
            // {
            //     claims.Add(new Claim("Role", role));
            // }

            // var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSetting:securityKey"]));
            // var token = new JwtSecurityToken(
            //     issuer: _configuration["JWTSetting:ValidIssuer"],
            //     audience: _configuration["JWTSetting:ValidAudience"],
            //     expires: DateTime.Now.AddMinutes(60),
            //     claims: claims,
            //     signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            // );

            // return new JwtSecurityTokenHandler().WriteToken(token);

            var token = new JwtTokenBuilder()
                .AddSecurityKey(JwtSecurityKey.Create(_configuration["JWTSetting:securityKey"]))
                .AddIssuer(_configuration["JWTSetting:ValidIssuer"])
                .AddAudience(_configuration["JWTSetting:ValidAudience"])
                .AddSubject(user.Id.ToString())
                .AddExpiry(60)
                .AddClaim("UserId", user.Id.ToString())
                .AddClaim("Email", user.Email ?? "")
                .Build();
            return token.Value;
        }

        public async Task<CommonResponse<ValidateTokenResponse>> ValidateToken(string token)
        {
            var response = new CommonResponse<ValidateTokenResponse>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSetting:securityKey"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWTSetting:ValidIssuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWTSetting:ValidAudience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                response.Success = true;
                Console.WriteLine($"pricipal ne cha: {principal}");
                var userId = principal?.FindFirst("UserId")?.Value;
                var email = principal?.FindFirst("Email")?.Value;
                response.Payload = new ValidateTokenResponse
                {
                    UserId = userId,
                    Email = email
                };
            }
            catch (SecurityTokenExpiredException)
            {
                response.Success = false;
                response.Error = new ErrorItem()
                {
                    Message = "Token has expired"
                };
            }
            catch (SecurityTokenException)
            {
                response.Success = false;
                response.Error = new ErrorItem()
                {
                    Message = "Token validation failed."
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = new ErrorItem()
                {
                    Message = $"Unexpected error: {ex.Message}"
                };
            }

            return response;
        }

    }
}