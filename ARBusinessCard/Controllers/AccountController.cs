using ARBusinessCard.Data;
using ARBusinessCard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace ARBusinessCard.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelper _db;

        public AccountController(DatabaseHelper db)
        {
            _db = db;
        }

        // ── GET: Login ────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Dashboard");
            return View(new LoginViewModel());
        }

        // ── POST: Login ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var hash = HashPassword(model.Password);
            var sql = "SELECT * FROM Users WHERE Username = @u AND PasswordHash = @p AND IsActive = 1";
            var dt = _db.ExecuteQuery(sql, new Dictionary<string, object>
            {
                { "@u", model.Username },
                { "@p", hash }
            });

            if (dt.Rows.Count == 0)
            {
                model.ErrorMessage = "Invalid username or password";
                return View(model);
            }

            var row = dt.Rows[0];
            HttpContext.Session.SetInt32("UserId", Convert.ToInt32(row["UserId"]));
            HttpContext.Session.SetString("UserName", row["FullName"].ToString()!);
            HttpContext.Session.SetString("Username", row["Username"].ToString()!);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── GET: Register ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Dashboard");
            return View(new RegisterViewModel());
        }

        // ── POST: Register ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check duplicate username/email
            var checkSql = "SELECT COUNT(*) FROM Users WHERE Username = @u OR Email = @e";
            var count = Convert.ToInt32(_db.ExecuteScalar(checkSql, new Dictionary<string, object>
            {
                { "@u", model.Username },
                { "@e", model.Email }
            }));

            if (count > 0)
            {
                model.ErrorMessage = "Username or Email already exists";
                return View(model);
            }

            var hash = HashPassword(model.Password);
            var insertSql = @"INSERT INTO Users (FullName, Email, Username, PasswordHash)
                              VALUES (@fn, @em, @un, @pw);
                              SELECT SCOPE_IDENTITY();";

            var newId = _db.ExecuteScalar(insertSql, new Dictionary<string, object>
            {
                { "@fn", model.FullName },
                { "@em", model.Email },
                { "@un", model.Username },
                { "@pw", hash }
            });

            HttpContext.Session.SetInt32("UserId", Convert.ToInt32(newId));
            HttpContext.Session.SetString("UserName", model.FullName);
            HttpContext.Session.SetString("Username", model.Username);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── Logout ────────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── Helper ────────────────────────────────────────────────
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
        }
    }
}
