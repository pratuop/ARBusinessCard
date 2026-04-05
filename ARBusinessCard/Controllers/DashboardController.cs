using ARBusinessCard.Data;
using ARBusinessCard.Models;
using Microsoft.AspNetCore.Mvc;

namespace ARBusinessCard.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DatabaseHelper _db;

        public DashboardController(DatabaseHelper db) => _db = db;

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // ── GET: Dashboard/Index ──────────────────────────────────
        public IActionResult Index()
        {
            if (GetUserId() == null) return RedirectToAction("Login", "Account");

            int uid = GetUserId()!.Value;

            // Get user info
            var userDt = _db.ExecuteQuery("SELECT * FROM Users WHERE UserId = @id",
                new Dictionary<string, object> { { "@id", uid } });

            var user = new User
            {
                UserId = uid,
                FullName = userDt.Rows[0]["FullName"].ToString()!,
                Email = userDt.Rows[0]["Email"].ToString()!,
                Username = userDt.Rows[0]["Username"].ToString()!
            };

            // Get cards with scan count
            var cardsSql = @"
                SELECT c.*, 
                       ISNULL((SELECT COUNT(*) FROM CardScans WHERE CardId = c.CardId), 0) AS ScanCount
                FROM Cards c 
                WHERE c.UserId = @uid AND c.IsActive = 1
                ORDER BY c.CreatedAt DESC";

            var cardsDt = _db.ExecuteQuery(cardsSql, new Dictionary<string, object> { { "@uid", uid } });
            var cards = new List<Card>();

            foreach (System.Data.DataRow row in cardsDt.Rows)
            {
                cards.Add(MapCard(row));
            }

            // Total scans
            var totalScans = Convert.ToInt32(_db.ExecuteScalar(
                @"SELECT ISNULL(SUM(s.ScanCount),0) FROM 
                  (SELECT COUNT(*) AS ScanCount FROM CardScans cs 
                   INNER JOIN Cards c ON cs.CardId = c.CardId WHERE c.UserId = @uid) s",
                new Dictionary<string, object> { { "@uid", uid } }));

            var vm = new DashboardViewModel
            {
                CurrentUser = user,
                Cards = cards,
                TotalCards = cards.Count,
                TotalScans = totalScans
            };

            return View(vm);
        }

        private static Card MapCard(System.Data.DataRow row) => new()
        {
            CardId = Convert.ToInt32(row["CardId"]),
            CardTitle = row["CardTitle"].ToString()!,
            FullName = row["FullName"].ToString()!,
            JobTitle = row["JobTitle"]?.ToString(),
            Company = row["Company"]?.ToString(),
            Email = row["Email"]?.ToString(),
            Phone = row["Phone"]?.ToString(),
            ProfilePhoto = row["ProfilePhoto"]?.ToString(),
            CardTheme = row["CardTheme"].ToString()!,
            UniqueSlug = row["UniqueSlug"].ToString()!,
            QRCodePath = row["QRCodePath"]?.ToString(),
            AvatarStyle = row["AvatarStyle"].ToString()!,
            CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
            ScanCount = Convert.ToInt32(row["ScanCount"])
        };
    }
}
