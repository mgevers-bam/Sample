using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Core.Domain;

[Table("PasswordResetTokens")]
public class PasswordResetToken
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UsedAt { get; set; }

    public bool IsUsed => UsedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsValid => !IsUsed && !IsExpired;
}
