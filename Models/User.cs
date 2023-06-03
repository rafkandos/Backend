using Microsoft.EntityFrameworkCore;

namespace CualiVy_CC.Models;

public class User
{
    public Guid guid { get; set; }
    public string fullname { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string? token { get; set; }
    public DateTime? createdat { get; set; }
    public DateTime? updatedat { get; set; }
}

public class Login
{
    public string email { get; set; }
    public string password { get; set; }
}

public class Register
{
    public string fullname { get; set; }
    public string email { get; set; }
    public string password { get; set; }
}