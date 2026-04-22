using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Core.Domain;

[Table("RefreshTokens")]
public class RefreshToken
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

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsValid => !IsRevoked && !IsExpired;
}

[Table("RevokedTokens")]
public class RevokedToken
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string JwtTokenId { get; set; } = string.Empty;

    [Required]
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public string? Reason { get; set; }
}
