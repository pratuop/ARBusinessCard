using ARBusinessCard.Data;
using ARBusinessCard.Models;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace ARBusinessCard.Controllers
{
    public class CardController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public CardController(DatabaseHelper db, IWebHostEnvironment env, IConfiguration config)
        {
            _db = db; _env = env; _config = config;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // ── GET: Create ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");
            return View(new CardViewModel());
        }

        // ── POST: Create ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CardViewModel model)
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View(model);

            int uid = GetUserId()!.Value;
            string slug = GenerateSlug(model.FullName);
            string? photoPath = await SavePhoto(model.ProfilePhotoFile);
            string qrPath = GenerateQRCode(slug);

            var sql = @"INSERT INTO Cards
                (UserId, CardTitle, FullName, JobTitle, Company, Email, Phone, Website,
                 LinkedIn, Twitter, Instagram, ProfilePhoto, AvatarStyle, AvatarSkinTone,
                 AvatarHair, AvatarOutfit, CardTheme, QRCodePath, UniqueSlug)
                VALUES (@uid,@ct,@fn,@jt,@co,@em,@ph,@wb,@li,@tw,@ig,@pp,@as,@ast,@ah,@ao,@th,@qr,@sl)";

            // ✅ FIX: null values properly handled with DBNull
            _db.ExecuteNonQuery(sql, new Dictionary<string, object>
            {
                {"@uid", uid},
                {"@ct", model.CardTitle},
                {"@fn", model.FullName},
                {"@jt", (object?)model.JobTitle ?? DBNull.Value},
                {"@co", (object?)model.Company ?? DBNull.Value},
                {"@em", (object?)model.Email ?? DBNull.Value},
                {"@ph", (object?)model.Phone ?? DBNull.Value},
                {"@wb", (object?)model.Website ?? DBNull.Value},
                {"@li", (object?)model.LinkedIn ?? DBNull.Value},
                {"@tw", (object?)model.Twitter ?? DBNull.Value},
                {"@ig", (object?)model.Instagram ?? DBNull.Value},
                {"@pp", (object?)photoPath ?? DBNull.Value},
                {"@as", model.AvatarStyle},
                {"@ast", model.AvatarSkinTone},
                {"@ah", model.AvatarHair},
                {"@ao", model.AvatarOutfit},
                {"@th", model.CardTheme},
                {"@qr", qrPath},
                {"@sl", slug}
            });

            TempData["Success"] = "Card created successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

        // ── GET: Edit ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");
            var card = GetCardById(id, GetUserId()!.Value);
            if (card == null) return NotFound();

            var vm = new CardViewModel
            {
                CardId = card.CardId,
                CardTitle = card.CardTitle,
                FullName = card.FullName,
                JobTitle = card.JobTitle,
                Company = card.Company,
                Email = card.Email,
                Phone = card.Phone,
                Website = card.Website,
                LinkedIn = card.LinkedIn,
                Twitter = card.Twitter,
                Instagram = card.Instagram,
                ExistingPhoto = card.ProfilePhoto,
                AvatarStyle = card.AvatarStyle,
                AvatarSkinTone = card.AvatarSkinTone,
                AvatarHair = card.AvatarHair,
                AvatarOutfit = card.AvatarOutfit,
                CardTheme = card.CardTheme
            };
            return View(vm);
        }

        // ── POST: Edit ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CardViewModel model)
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) return View(model);

            string? photoPath = model.ExistingPhoto;
            if (model.ProfilePhotoFile != null)
                photoPath = await SavePhoto(model.ProfilePhotoFile);

            var sql = @"UPDATE Cards SET
                        CardTitle=@ct, FullName=@fn, JobTitle=@jt, Company=@co,
                        Email=@em, Phone=@ph, Website=@wb, LinkedIn=@li, Twitter=@tw,
                        Instagram=@ig, ProfilePhoto=@pp, AvatarStyle=@as, AvatarSkinTone=@ast,
                        AvatarHair=@ah, AvatarOutfit=@ao, CardTheme=@th, UpdatedAt=GETDATE()
                        WHERE CardId=@id AND UserId=@uid";

            // ✅ FIX: null values properly handled
            _db.ExecuteNonQuery(sql, new Dictionary<string, object>
            {
                {"@ct",  model.CardTitle},
                {"@fn",  model.FullName},
                {"@jt",  (object?)model.JobTitle ?? DBNull.Value},
                {"@co",  (object?)model.Company ?? DBNull.Value},
                {"@em",  (object?)model.Email ?? DBNull.Value},
                {"@ph",  (object?)model.Phone ?? DBNull.Value},
                {"@wb",  (object?)model.Website ?? DBNull.Value},
                {"@li",  (object?)model.LinkedIn ?? DBNull.Value},
                {"@tw",  (object?)model.Twitter ?? DBNull.Value},
                {"@ig",  (object?)model.Instagram ?? DBNull.Value},
                {"@pp",  (object?)photoPath ?? DBNull.Value},
                {"@as",  model.AvatarStyle},
                {"@ast", model.AvatarSkinTone},
                {"@ah",  model.AvatarHair},
                {"@ao",  model.AvatarOutfit},
                {"@th",  model.CardTheme},
                {"@id",  model.CardId},
                {"@uid", GetUserId()!.Value}
            });

            TempData["Success"] = "Card updated!";
            return RedirectToAction("Index", "Dashboard");
        }

        // ── POST: Delete ──────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Delete(int id)
        {
            if (GetUserId() == null) return Json(new { success = false });
            _db.ExecuteNonQuery(
                "UPDATE Cards SET IsActive=0 WHERE CardId=@id AND UserId=@uid",
                new Dictionary<string, object> { { "@id", id }, { "@uid", GetUserId()!.Value } });
            return Json(new { success = true });
        }

        // ── GET: Public AR View (QR scan landing page) ────────────
        [HttpGet]
        [ActionName("View")]
        public IActionResult PublicCard(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var dt = _db.ExecuteQuery(
                "SELECT * FROM Cards WHERE UniqueSlug=@s AND IsActive=1",
                new Dictionary<string, object> { { "@s", slug } });

            if (dt.Rows.Count == 0) return NotFound();

            var card = MapCard(dt.Rows[0]);

            // Log QR scan
            try
            {
                _db.ExecuteNonQuery(
                    "INSERT INTO CardScans (CardId, IPAddress, UserAgent) VALUES (@cid, @ip, @ua)",
                    new Dictionary<string, object>
                    {
                        {"@cid", card.CardId},
                        {"@ip",  HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""},
                        {"@ua",  Request.Headers["User-Agent"].ToString()}
                    });
            }
            catch { /* scan logging failure should not break the page */ }

            var vm = new CardViewViewModel
            {
                Card = card,
                BaseUrl = $"{Request.Scheme}://{Request.Host}"
            };

            return View("View", vm);
        }

        // ── GET: Download QR PNG ──────────────────────────────────
        public IActionResult Download(int id)
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");
            var card = GetCardById(id, GetUserId()!.Value);
            if (card == null || string.IsNullOrEmpty(card.QRCodePath)) return NotFound();

            var qrFullPath = Path.Combine(_env.WebRootPath, card.QRCodePath.TrimStart('/'));
            if (!System.IO.File.Exists(qrFullPath)) return NotFound();

            var bytes = System.IO.File.ReadAllBytes(qrFullPath);
            return File(bytes, "image/png", $"QR_{card.CardTitle}.png");
        }

        // ── Helpers ───────────────────────────────────────────────
        private Card? GetCardById(int id, int uid)
        {
            var dt = _db.ExecuteQuery(
                "SELECT * FROM Cards WHERE CardId=@id AND UserId=@uid AND IsActive=1",
                new Dictionary<string, object> { { "@id", id }, { "@uid", uid } });
            if (dt.Rows.Count == 0) return null;
            return MapCard(dt.Rows[0]);
        }

        private string GenerateSlug(string name)
        {
            var clean = new string(
                name.ToLower().Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray())
                .Replace(' ', '-');
            return $"{clean}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        private string GenerateQRCode(string slug)
        {
            var baseUrl = _config["AppSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/c/{slug}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(10);

            var folder = Path.Combine(_env.WebRootPath, "qrcodes");
            Directory.CreateDirectory(folder);
            var fileName = $"{slug}.png";
            System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), bytes);

            return $"/qrcodes/{fileName}";
        }

        private async Task<string?> SavePhoto(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var folder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{fileName}";
        }

        private static Card MapCard(System.Data.DataRow row) => new()
        {
            CardId = Convert.ToInt32(row["CardId"]),
            UserId = Convert.ToInt32(row["UserId"]),
            CardTitle = row["CardTitle"].ToString()!,
            FullName = row["FullName"].ToString()!,
            JobTitle = row["JobTitle"] == DBNull.Value ? null : row["JobTitle"]?.ToString(),
            Company = row["Company"] == DBNull.Value ? null : row["Company"]?.ToString(),
            Email = row["Email"] == DBNull.Value ? null : row["Email"]?.ToString(),
            Phone = row["Phone"] == DBNull.Value ? null : row["Phone"]?.ToString(),
            Website = row["Website"] == DBNull.Value ? null : row["Website"]?.ToString(),
            LinkedIn = row["LinkedIn"] == DBNull.Value ? null : row["LinkedIn"]?.ToString(),
            Twitter = row["Twitter"] == DBNull.Value ? null : row["Twitter"]?.ToString(),
            Instagram = row["Instagram"] == DBNull.Value ? null : row["Instagram"]?.ToString(),
            ProfilePhoto = row["ProfilePhoto"] == DBNull.Value ? null : row["ProfilePhoto"]?.ToString(),
            AvatarStyle = row["AvatarStyle"].ToString()!,
            AvatarSkinTone = row["AvatarSkinTone"].ToString()!,
            AvatarHair = row["AvatarHair"].ToString()!,
            AvatarOutfit = row["AvatarOutfit"].ToString()!,
            CardTheme = row["CardTheme"].ToString()!,
            QRCodePath = row["QRCodePath"] == DBNull.Value ? null : row["QRCodePath"]?.ToString(),
            UniqueSlug = row["UniqueSlug"].ToString()!,
            CreatedAt = Convert.ToDateTime(row["CreatedAt"])
        };
    }
}