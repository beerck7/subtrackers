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
        public DbSet<User> Users { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }
        public DbSet<FamilyConnection> FamilyConnections { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "subscriptions.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();


            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<PaymentLog>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.PaymentLogs)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<PaymentLog>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<FamilyConnection>()
                .HasOne(fc => fc.SenderUser)
                .WithMany()
                .HasForeignKey(fc => fc.SenderUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FamilyConnection>()
                .HasOne(fc => fc.ReceiverUser)
                .WithMany()
                .HasForeignKey(fc => fc.ReceiverUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
