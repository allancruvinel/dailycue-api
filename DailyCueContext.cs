using System;
using Microsoft.EntityFrameworkCore;

namespace dailycue_api;

public class DailyCueContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=dailycue;Username=yourusername;Password=yourpassword");
    }

}
