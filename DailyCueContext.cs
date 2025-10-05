using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace dailycue_api;

public class DailyCueContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DailyCueContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
    }

    public DbSet<Entities.User> Users { get; set; }
    public DbSet<Entities.Chat> Chats { get; set; }
    public DbSet<Entities.Tip> Tips { get; set; }
    public DbSet<Entities.TipSchedule> TipSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }
}
