using Microsoft.EntityFrameworkCore;
using BotPoe.Models;
using System;

namespace BotPoe.Data;

public class AppDbContext : DbContext
{
    public DbSet<RegexHistory> RegexHistories => Set<RegexHistory>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var pass = Environment.GetEnvironmentVariable("DB_PASS");
        
        string connectionString = $"Host={host};Port={port};Database={dbName};Username={user};Password={pass}";
        
        options.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegexHistory>().HasKey(r => new { r.UserId, r.Name });
        base.OnModelCreating(modelBuilder);
    }
}