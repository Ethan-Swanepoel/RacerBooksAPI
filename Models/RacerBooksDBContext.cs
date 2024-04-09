using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RacerBooksAPI.Models;

public partial class RacerBooksDBContext : DbContext
{
    public RacerBooksDBContext()
    {
    }

    public RacerBooksDBContext(DbContextOptions<RacerBooksDBContext> options)
        : base(options)
    {
    }

}
