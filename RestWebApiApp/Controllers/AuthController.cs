using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestWebApiApp.Data;
using RestWebApiApp.Services;
using RestWebApiApp.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace RestWebApiApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRep userRep;
        private readonly IConfiguration configuration;

        public AuthController(IUserRep userRep, IConfiguration configuration)
        {
            this.userRep = userRep;
            this.configuration = configuration;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            var user = await userRep.GetUserByUsernameAsync(request.UserName);
            if (user != null || request.UserName.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
            {
                return BadRequest();
            }

            var salt = PasswordHashService.GenerateSalt();
            string passwordHash = PasswordHashService.HashPassword(request.Password, salt);
            await userRep.AddUserAsync(new Models.User()
            {
                UserName = request.UserName,
                PasswordHash = passwordHash,
                RefreshToken = TokenService.GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.Now.AddDays(30),
                Salt = salt
            });

            var accessToken = TokenService.GenerateJwtToken(request.UserName, configuration["Jwt:Key"]);
            var refreshToken = TokenService.GenerateRefreshToken();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true
            };

            Response.Cookies.Append("access_token", accessToken, cookieOptions);
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                Path = "/refresh",
                HttpOnly = true
            });

            return StatusCode(201);
        }

        [HttpPost("/auth")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            var user = await userRep.GetUserByUsernameAsync(request.UserName);
            if (user != null)
            {
                string givenPassHash = PasswordHashService.HashPassword(request.Password, user.Salt);

                if (givenPassHash == user.PasswordHash)
                {
                    var accessToken = TokenService.GenerateJwtToken(request.UserName, configuration["Jwt:Key"]);
                    var refreshToken = TokenService.GenerateRefreshToken();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(30);
                    await userRep.UpdateUserAsync(user);

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true
                    };

                    Response.Cookies.Append("access_token", accessToken, cookieOptions);
                    Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
                    {
                        Path = "/refresh",
                        HttpOnly = true
                    });

                    return Ok();
                }
            }

            return Forbid();
        }

        [HttpPost("/refresh")]
        public async Task<IActionResult> RefreshTokens()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            var user = await userRep.GetUserByRefToken(refreshToken ?? String.Empty);

            if (user != null)
            {
                if (user.RefreshTokenExpiryTime >= DateTime.UtcNow)
                {
                    var newAccessToken = TokenService.GenerateJwtToken(user.UserName, configuration["Jwt:Key"]);
                    var newRefreshToken = TokenService.GenerateRefreshToken();

                    user.RefreshToken = newRefreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(30);
                    await userRep.UpdateUserAsync(user);

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true
                    };

                    Response.Cookies.Append("access_token", newAccessToken, cookieOptions);
                    Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
                    {
                        Path = "/refresh",
                        HttpOnly = true
                    });

                    return Ok();
                }
            }

            return Forbid();
        }
    }
}