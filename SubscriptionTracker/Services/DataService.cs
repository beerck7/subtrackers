using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionTracker.Services
{
    public class DataService
    {
        // ================= USER AUTHENTICATION =================

        public async Task<User> LoginUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            using (var db = new AppDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
                if (user == null) return null;

                if (PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    return user;
                }
                return null;
            }
        }

        public async Task<bool> RegisterUserAsync(string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            using (var db = new AppDbContext())
            {
                var exists = await db.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
                if (exists) return false;

                var passwordHash = PasswordHasher.HashPassword(password);
                var user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Email = email
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                // Seed default categories specifically for this user
                var defaultCategories = new List<Category>
                {
                    new Category { Name = "Streaming", Color = "#22c55e", UserId = user.Id },
                    new Category { Name = "Muzyka", Color = "#f43f5e", UserId = user.Id },
                    new Category { Name = "Oprogramowanie", Color = "#3b82f6", UserId = user.Id },
                    new Category { Name = "Gry", Color = "#eab308", UserId = user.Id },
                    new Category { Name = "Inne", Color = "#a855f7", UserId = user.Id }
                };

                db.Categories.AddRange(defaultCategories);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            using (var db = new AppDbContext())
            {
                db.Users.Update(user);
                await db.SaveChangesAsync();
            }
        }

        // ================= CATEGORIES =================

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                return await db.Categories
                    .Include(c => c.Subscriptions)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            category.UserId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                var hasSubscriptions = await db.Subscriptions.AnyAsync(s => s.CategoryId == id && s.UserId == userId);
                if (hasSubscriptions)
                {
                    return false;
                }

                var cat = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
                if (cat != null)
                {
                    db.Categories.Remove(cat);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }

        // ================= SUBSCRIPTIONS =================

        public async Task<List<Subscription>> GetSubscriptionsAsync()
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                return await db.Subscriptions
                    .Include(s => s.Category)
                    .Where(s => s.UserId == userId)
                    .ToListAsync();
            }
        }

        public async Task AddSubscriptionAsync(Subscription subscription)
        {
            subscription.UserId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                db.Subscriptions.Add(subscription);
                if (subscription.Category != null)
                {
                    db.Entry(subscription.Category).State = EntityState.Unchanged;
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateSubscriptionAsync(Subscription subscription)
        {
            subscription.UserId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                db.Subscriptions.Update(subscription);
                if (subscription.Category != null)
                {
                    db.Entry(subscription.Category).State = EntityState.Unchanged;
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteSubscriptionAsync(int id)
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                var sub = await db.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
                if (sub != null)
                {
                    db.Subscriptions.Remove(sub);
                    await db.SaveChangesAsync();
                }
            }
        }

        // ================= PAYMENT LOGS =================

        public async Task<List<PaymentLog>> GetPaymentLogsAsync()
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                return await db.PaymentLogs
                    .Include(p => p.Subscription)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }
        }

        public async Task<bool> LogSubscriptionPaymentAsync(int subscriptionId, string note)
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                var sub = await db.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);
                if (sub == null) return false;

                var log = new PaymentLog
                {
                    SubscriptionId = sub.Id,
                    UserId = userId,
                    PaymentDate = System.DateTime.Now,
                    AmountPaid = sub.SplitPrice,
                    Note = string.IsNullOrWhiteSpace(note) ? $"Opłacono: {sub.Name} ({sub.Cycle})" : note
                };

                db.PaymentLogs.Add(log);

                // Advance NextPaymentDate automatically based on the cycle type
                if (sub.Cycle == "Rocznie")
                {
                    sub.NextPaymentDate = sub.NextPaymentDate.AddYears(1);
                }
                else
                {
                    sub.NextPaymentDate = sub.NextPaymentDate.AddMonths(1);
                }

                await db.SaveChangesAsync();
                return true;
            }
        }
        // ================= FAMILY MEMBERS =================

        public async Task<List<FamilyMember>> GetFamilyMembersAsync()
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                return await db.FamilyMembers
                    .Where(f => f.UserId == userId)
                    .ToListAsync();
            }
        }

        public async Task AddFamilyMemberAsync(FamilyMember member)
        {
            member.UserId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                db.FamilyMembers.Add(member);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteFamilyMemberAsync(int id)
        {
            var userId = SessionManager.CurrentUser?.Id ?? 0;
            using (var db = new AppDbContext())
            {
                var member = await db.FamilyMembers.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
                if (member != null)
                {
                    db.FamilyMembers.Remove(member);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
