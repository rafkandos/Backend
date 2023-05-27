using Microsoft.AspNetCore.Mvc;
using CualiVy_CC.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CualiVy_CC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly CualiVyContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(CualiVyContext DBContext, IConfiguration configuration)
    {
        this._context = DBContext;
        _configuration = configuration;
    }

    [HttpGet("GetUsers")]
    public async Task<ActionResult<ReturnAPI>> Get()
    {
        var rtn = new ReturnAPI();
        var listData = _context.User.ToList();

        rtn.data = listData;
        return rtn;
    }

    [HttpPost("register")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<ReturnAPI>> Register([FromForm] Register user)
    {
        var rtn = new ReturnAPI();
        try
        {
            var checkUserExist = _context.User.Where(i => i.email == user.email).ToList();
            if (checkUserExist.Count > 0) throw new Exception("Email already used!");

            var newUser = new User()
            {
                guid = Guid.NewGuid(),
                fullname = user.fullname,
                email = user.email,
                password = HashPassword(user.password),
                createdat = DateTime.Now
            };

            await _context.User.AddAsync(newUser);
            await _context.SaveChangesAsync();

            rtn.status = 201;
            rtn.message = "Created";
        }
        catch (Exception e)
        {
            rtn.status = 500;
            rtn.message = e.Message;
        }

        return rtn;
    }

    [HttpPost("login")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<ReturnAPI>> Login([FromForm]Login login)
    {
        var rtn = new ReturnAPI();
        try
        {
            var checkUser = _context.User
                                .Where(i => i.email == login.email)
                                .ToList();
            if (checkUser.Count < 1) throw new Exception("User not found!");

            bool passwordMatches = VerifyPassword(login.password, checkUser[0].password);
            if (!passwordMatches) throw new Exception("Wrong password!");

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, login.email),
                    new Claim(JwtRegisteredClaimNames.Jti, checkUser[0].guid.ToString()),
                };

            var token = GetToken(authClaims);

            var updToken = checkUser[0];
            updToken.token = new JwtSecurityTokenHandler().WriteToken(token);
            updToken.updatedat = DateTime.Now;

            _context.Update(updToken);
            await _context.SaveChangesAsync();

            rtn.data = new
            {
                token = updToken.token
            };
        }
        catch (Exception e)
        {
            rtn.status = 500;
            rtn.message = e.Message;
        }

        return rtn;
    }

    public static string HashPassword(string password)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            hashedPassword = "c" + hashedPassword + "vy";

            return hashedPassword;
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            authClaims,
            expires: DateTime.UtcNow.AddYears(10),
            signingCredentials: signIn);

        return token;
    }
}