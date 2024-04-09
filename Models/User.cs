using System;
using System.Collections.Generic;

namespace RacerBooksAPI.Models;

public partial class User
{
    public string Email { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public string? FirebaseUuid { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
