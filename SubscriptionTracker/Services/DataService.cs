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
    }
}
