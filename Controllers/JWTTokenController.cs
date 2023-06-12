using DemoJWT.Context;
using DemoJWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JWTTokenController : ControllerBase
    {
        public IConfiguration _configuration;
        public readonly ApplicationDBContext _context;
        public JWTTokenController(IConfiguration configuration, ApplicationDBContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        
        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
            if (user != null && user.UserName != null && user.Password != null)
            {
                var userData = await GetUser(user.UserName, user.Password);
                var jwt = _configuration.GetSection("Jwt").Get<Jwt>();
                if (user != null)
                {
                    var claims = new[]
                    {
                    new Claim(JwtRegisteredClaimNames.Sub , jwt.Subject),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("Id" , user.UserId.ToString()),
                    new Claim("UserName" , user.UserName),
                    new Claim("Password", user.Password)
                };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        jwt.Issuer,
                        jwt.Audience,
                        claims,
                        expires: DateTime.Now.AddMinutes(50),
                        signingCredentials: signIn
                        );
                    _context.Add(user);
                    _context.SaveChanges();
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return BadRequest("Invalid Credentials");
                }
            }
            else
            {
                return BadRequest();
            }
            

        }
        [HttpGet]
        public async Task<User>GetUser(string username , string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
            return user;
        }
    }
}
