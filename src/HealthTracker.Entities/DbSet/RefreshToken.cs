using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthTracker.Entities.DbSet;

public class RefreshToken : BaseEntity
{
    public string  UserId { get; set; } // User Id when logged in
    public string Token { get; set; }
    public string JwtId { get; set; } // the id generated when a jwt Id has been requested
    public bool IsUsed { get; set; } // To make sure that the token is only used once
    public bool IsRevoked { get; set; } // Make sure they are valid
    public DateTime ExpiryDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; }
}
