using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenSwamp.Data;
using GreenSwamp.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GreenSwamp.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /account/login
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return Redirect("/feed");

            
            return RedirectToPage("/login");
        }

        // POST /account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .Include(u => u.Auth)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user?.Auth == null || !VerifyPassword(password, user.Auth.PasswordHash))
            {
                
                TempData["Error"] = "Неверный логин или пароль";
                return RedirectToPage("/login");
            }

            
            user.Auth.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SignInUser(user);
            return Redirect("/feed");
        }

        // GET /account/register
        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return Redirect("/feed");

            
            return RedirectToPage("/register");
        }

        // POST /account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string displayName, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Все поля обязательны для заполнения";
                return RedirectToPage("/register");
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Пароли не совпадают";
                return RedirectToPage("/register");
            }

            if (password.Length < 6)
            {
                TempData["Error"] = "Пароль должен быть минимум 6 символов";
                return RedirectToPage("/register");
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                TempData["Error"] = "Пользователь с таким именем уже существует";
                return RedirectToPage("/register");
            }

            var user = new User
            {
                Username = username,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Auth = new Auth
                {
                    PasswordHash = HashPassword(password),
                    LastLogin = DateTime.UtcNow
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInUser(user);
            return Redirect("/feed");
        }

        // GET /account/logout
        [HttpGet("logout")]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            
            if (!string.IsNullOrEmpty(returnUrl))
            {
                
                var decodedUrl = Uri.UnescapeDataString(returnUrl);

                
                if (decodedUrl.StartsWith("/"))
                {
                    return Redirect(decodedUrl);
                }
            }

            
            return Redirect("/feed");
        }

        

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("DisplayName", user.DisplayName),
                new Claim("AvatarUrl", user.AvatarUrl ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                }
            );
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "greenswamp_salt"));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}