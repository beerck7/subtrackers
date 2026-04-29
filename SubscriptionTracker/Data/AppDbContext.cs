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
                new Category { Id = 1, Name = "Streaming" },
                new Category { Id = 2, Name = "Muzyka" },
                new Category { Id = 3, Name = "Oprogramowanie" },
                new Category { Id = 4, Name = "Gry" },
                new Category { Id = 5, Name = "Inne" }
            );
        }
    }
}
