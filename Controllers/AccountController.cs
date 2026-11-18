using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuizHub.Data;
using QuizHub.Models;

namespace QuizHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<QuizHubUser> _hasher;

        public AccountController(
            ApplicationDbContext context,
            IPasswordHasher<QuizHubUser> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string username,
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            if (username.Length > 50 || email.Length > 255 ||
                password.Length < 8)
            {
                ModelState.AddModelError("",
                    "Check your input. Password must be at least 8 characters.");
                return View();
            }

            bool exists = await _context.Users.AnyAsync(u =>
                u.Username == username || u.Email == email);

            if (exists)
            {
                ModelState.AddModelError("",
                    "Username or email is already registered.");
                return View();
            }

            var user = new QuizHubUser
            {
                Username = username.Trim(),
                Email = email.Trim()
            };

            user.PasswordHash = _hasher.HashPassword(
                user, password
            );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInUser(user);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == username || u.Email == username);

            if (user == null)
            {
                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View();
            }

            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View();
            }

            await SignInUser(user);

            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(QuizHubUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(ClaimTypes.Name,
                    user.Username),

                new Claim(ClaimTypes.Email,
                    user.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                "Cookies"
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "Cookies",
                principal
            );
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Login");
        }
    }
}
