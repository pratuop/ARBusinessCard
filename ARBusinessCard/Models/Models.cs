using System.ComponentModel.DataAnnotations;

namespace ARBusinessCard.Models
{
    // ─── USER MODEL ───────────────────────────────────────────────
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? ProfilePhoto { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // ─── CARD MODEL ───────────────────────────────────────────────
    public class Card
    {
        public int CardId { get; set; }
        public int UserId { get; set; }
        public string CardTitle { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? JobTitle { get; set; }
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? LinkedIn { get; set; }
        public string? Twitter { get; set; }
        public string? Instagram { get; set; }
        public string? ProfilePhoto { get; set; }
        public string AvatarStyle { get; set; } = "style1";
        public string AvatarSkinTone { get; set; } = "light";
        public string AvatarHair { get; set; } = "short";
        public string AvatarOutfit { get; set; } = "business";
        public string CardTheme { get; set; } = "cyber";
        public string? QRCodePath { get; set; }
        public string UniqueSlug { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ScanCount { get; set; }
    }

    // ─── LOGIN VIEWMODEL ──────────────────────────────────────────
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // ─── REGISTER VIEWMODEL ───────────────────────────────────────
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = "";

        [Required]
        [StringLength(100, MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        public string? ErrorMessage { get; set; }
    }

    // ─── DASHBOARD VIEWMODEL ──────────────────────────────────────
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; } = new();
        public List<Card> Cards { get; set; } = new();
        public int TotalCards { get; set; }
        public int TotalScans { get; set; }
    }

    // ─── CARD CREATE/EDIT VIEWMODEL ───────────────────────────────
    public class CardViewModel
    {
        public int CardId { get; set; }

        [Required(ErrorMessage = "Card title is required")]
        public string CardTitle { get; set; } = "";

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = "";

        public string? JobTitle { get; set; }
        public string? Company { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? LinkedIn { get; set; }
        public string? Twitter { get; set; }
        public string? Instagram { get; set; }
        public string? ExistingPhoto { get; set; }
        public IFormFile? ProfilePhotoFile { get; set; }

        // Avatar customization
        public string AvatarStyle { get; set; } = "style1";
        public string AvatarSkinTone { get; set; } = "light";
        public string AvatarHair { get; set; } = "short";
        public string AvatarOutfit { get; set; } = "business";

        // Theme
        public string CardTheme { get; set; } = "cyber";
    }

    // ─── CARD VIEW (PUBLIC AR PAGE) ───────────────────────────────
    public class CardViewViewModel
    {
        public Card Card { get; set; } = new();
        public string BaseUrl { get; set; } = "";
    }
}
