using System;
using System.Collections.Generic;

namespace WebApiTraining1.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public int? UserId { get; set; }

    public string Token { get; set; } = null!;

    public string JwtId { get; set; } = null!;

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiredAt { get; set; }

    public virtual User? User { get; set; }
}
