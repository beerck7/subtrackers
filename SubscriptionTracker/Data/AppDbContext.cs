using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Models;
using System;
using System.IO;

namespace SubscriptionTracker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "subscriptions.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacja
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(s => s.CategoryId);

            // Dane początkowe (Seed)
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Streaming", Color = "#22c55e" },
                new Category { Id = 2, Name = "Muzyka", Color = "#f43f5e" },
                new Category { Id = 3, Name = "Oprogramowanie", Color = "#3b82f6" },
                new Category { Id = 4, Name = "Gry", Color = "#eab308" },
                new Category { Id = 5, Name = "Inne", Color = "#a855f7" }
            );
        }
    }
}
