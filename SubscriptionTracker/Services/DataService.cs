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
                db.Entry(subscription.Category).State = EntityState.Unchanged;
                await db.SaveChangesAsync();
            }
        }
    }
}
