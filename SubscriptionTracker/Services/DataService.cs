using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubscriptionTracker.Services
{
    public class DataService
    {
        public async Task<List<Category>> GetCategoriesAsync()
        {
            using (var db = new AppDbContext())
            {
                return await db.Categories
                    .Include(c => c.Subscriptions)
                    .ToListAsync();
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            using (var db = new AppDbContext())
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Subscription>> GetSubscriptionsAsync()
        {
            using (var db = new AppDbContext())
            {
                return await db.Subscriptions
                    .Include(s => s.Category)
                    .ToListAsync();
            }
        }

        public async Task AddSubscriptionAsync(Subscription subscription)
        {
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
            using (var db = new AppDbContext())
            {
                var sub = await db.Subscriptions.FindAsync(id);
                if (sub != null)
                {
                    db.Subscriptions.Remove(sub);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using (var db = new AppDbContext())
            {
                var hasSubscriptions = await db.Subscriptions.AnyAsync(s => s.CategoryId == id);
                if (hasSubscriptions)
                {
                    return false;
                }

                var cat = await db.Categories.FindAsync(id);
                if (cat != null)
                {
                    db.Categories.Remove(cat);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }
    }
}
