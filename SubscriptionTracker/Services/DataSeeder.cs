using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SubscriptionTracker.Data;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;

namespace SubscriptionTracker.Services
{
    public static class DataSeeder
    {
        public static void SeedData(AppDbContext db)
        {
            if (db.Users.Any()) return;


            var user1 = new User
            {
                Username = "jan",
                Email = "jan.kowalski@gmail.com",
                PasswordHash = PasswordHasher.HashPassword("jan123")
            };
            db.Users.Add(user1);


            var user2 = new User
            {
                Username = "anna",
                Email = "anna.nowak@gmail.com",
                PasswordHash = PasswordHasher.HashPassword("anna123")
            };
            db.Users.Add(user2);

            db.SaveChanges();


            var cat1_streaming = new Category { Name = "Streaming", Color = "#22c55e", UserId = user1.Id };
            var cat1_muzyka = new Category { Name = "Muzyka", Color = "#f43f5e", UserId = user1.Id };
            var cat1_software = new Category { Name = "Oprogramowanie", Color = "#3b82f6", UserId = user1.Id };
            var cat1_gry = new Category { Name = "Gry", Color = "#eab308", UserId = user1.Id };
            var cat1_inne = new Category { Name = "Inne", Color = "#a855f7", UserId = user1.Id };

            db.Categories.AddRange(cat1_streaming, cat1_muzyka, cat1_software, cat1_gry, cat1_inne);


            var cat2_streaming = new Category { Name = "Streaming", Color = "#22c55e", UserId = user2.Id };
            var cat2_muzyka = new Category { Name = "Muzyka", Color = "#f43f5e", UserId = user2.Id };
            var cat2_software = new Category { Name = "Oprogramowanie", Color = "#3b82f6", UserId = user2.Id };
            var cat2_gry = new Category { Name = "Gry", Color = "#eab308", UserId = user2.Id };
            var cat2_inne = new Category { Name = "Inne", Color = "#a855f7", UserId = user2.Id };

            db.Categories.AddRange(cat2_streaming, cat2_muzyka, cat2_software, cat2_gry, cat2_inne);

            db.SaveChanges();



            var sub1_netflix = new Subscription
            {
                Name = "Netflix Premium",
                Price = 60.00M,
                Cycle = "Miesięcznie",
                StartDate = new DateTime(2026, 1, 10),
                NextPaymentDate = new DateTime(2026, 6, 10),
                CategoryId = cat1_streaming.Id,
                Status = "Aktywna",
                Note = "Współdzielony Netflix Premium 4K z Anną i Kasią.",
                UserId = user1.Id,
                IsShared = true,
                NumberOfMembers = 3,
                SharedWith = "Anna, Kasia"
            };


            var sub1_spotify = new Subscription
            {
                Name = "Spotify Premium",
                Price = 29.99M,
                Cycle = "Miesięcznie",
                StartDate = new DateTime(2026, 2, 15),
                NextPaymentDate = new DateTime(2026, 6, 15),
                CategoryId = cat1_muzyka.Id,
                Status = "Aktywna",
                Note = "Konto indywidualne.",
                UserId = user1.Id,
                IsShared = false,
                NumberOfMembers = 1,
                SharedWith = string.Empty
            };


            var sub1_m365 = new Subscription
            {
                Name = "Microsoft 365 Personal",
                Price = 439.99M,
                Cycle = "Rocznie",
                StartDate = new DateTime(2025, 12, 20),
                NextPaymentDate = new DateTime(2026, 12, 20),
                CategoryId = cat1_software.Id,
                Status = "Aktywna",
                Note = "Dysk OneDrive 1TB i pakiet biurowy.",
                UserId = user1.Id,
                IsShared = false,
                NumberOfMembers = 1,
                SharedWith = string.Empty
            };


            var sub1_prime = new Subscription
            {
                Name = "Amazon Prime Video",
                Price = 49.00M,
                Cycle = "Rocznie",
                StartDate = new DateTime(2025, 10, 5),
                NextPaymentDate = new DateTime(2026, 10, 5),
                CategoryId = cat1_streaming.Id,
                Status = "Aktywna",
                Note = "Pakiet roczny Amazon Prime dzielony z Tomkiem.",
                UserId = user1.Id,
                IsShared = true,
                NumberOfMembers = 2,
                SharedWith = "Tomek"
            };

            db.Subscriptions.AddRange(sub1_netflix, sub1_spotify, sub1_m365, sub1_prime);



            var sub2_disney = new Subscription
            {
                Name = "Disney+",
                Price = 38.99M,
                Cycle = "Miesięcznie",
                StartDate = new DateTime(2026, 2, 20),
                NextPaymentDate = new DateTime(2026, 6, 20),
                CategoryId = cat2_streaming.Id,
                Status = "Aktywna",
                Note = "Disney dzielony na pół z Janem.",
                UserId = user2.Id,
                IsShared = true,
                NumberOfMembers = 2,
                SharedWith = "Jan"
            };


            var sub2_psplus = new Subscription
            {
                Name = "PlayStation Plus Premium",
                Price = 400.00M,
                Cycle = "Rocznie",
                StartDate = new DateTime(2025, 9, 10),
                NextPaymentDate = new DateTime(2026, 9, 10),
                CategoryId = cat2_gry.Id,
                Status = "Aktywna",
                Note = "Gry online i katalog klasyków.",
                UserId = user2.Id,
                IsShared = false,
                NumberOfMembers = 1,
                SharedWith = string.Empty
            };


            var sub2_yt = new Subscription
            {
                Name = "YouTube Premium",
                Price = 25.99M,
                Cycle = "Miesięcznie",
                StartDate = new DateTime(2026, 3, 5),
                NextPaymentDate = new DateTime(2026, 6, 5),
                CategoryId = cat2_muzyka.Id,
                Status = "Aktywna",
                Note = "Brak reklam i YouTube Music.",
                UserId = user2.Id,
                IsShared = false,
                NumberOfMembers = 1,
                SharedWith = string.Empty
            };

            db.Subscriptions.AddRange(sub2_disney, sub2_psplus, sub2_yt);

            db.SaveChanges();



            var logs_jan = new[]
            {
                new PaymentLog { SubscriptionId = sub1_netflix.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 1, 10, 10, 30, 0), AmountPaid = 20.00M, Note = "Opłacenie cyklu Netflix Premium (Udział 1/3)" },
                new PaymentLog { SubscriptionId = sub1_netflix.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 2, 10, 11, 0, 0), AmountPaid = 20.00M, Note = "Opłacenie cyklu Netflix Premium (Udział 1/3)" },
                new PaymentLog { SubscriptionId = sub1_netflix.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 3, 10, 09, 15, 0), AmountPaid = 20.00M, Note = "Opłacenie cyklu Netflix Premium (Udział 1/3)" },
                new PaymentLog { SubscriptionId = sub1_netflix.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 4, 10, 14, 20, 0), AmountPaid = 20.00M, Note = "Opłacenie cyklu Netflix Premium (Udział 1/3)" },
                new PaymentLog { SubscriptionId = sub1_netflix.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 5, 10, 08, 0, 0), AmountPaid = 20.00M, Note = "Opłacenie cyklu Netflix Premium (Udział 1/3)" },


                new PaymentLog { SubscriptionId = sub1_spotify.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 3, 15, 12, 0, 0), AmountPaid = 29.99M, Note = "Opłacenie abonamentu Spotify" },
                new PaymentLog { SubscriptionId = sub1_spotify.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 4, 15, 13, 40, 0), AmountPaid = 29.99M, Note = "Opłacenie abonamentu Spotify" },
                new PaymentLog { SubscriptionId = sub1_spotify.Id, UserId = user1.Id, PaymentDate = new DateTime(2026, 5, 15, 10, 10, 0), AmountPaid = 29.99M, Note = "Opłacenie abonamentu Spotify" },


                new PaymentLog { SubscriptionId = sub1_m365.Id, UserId = user1.Id, PaymentDate = new DateTime(2025, 12, 20, 15, 0, 0), AmountPaid = 439.99M, Note = "Roczna opłata licencyjna" },


                new PaymentLog { SubscriptionId = sub1_prime.Id, UserId = user1.Id, PaymentDate = new DateTime(2025, 10, 5, 18, 0, 0), AmountPaid = 24.50M, Note = "Opłata roczna Prime Video (Udział 1/2)" }
            };
            db.PaymentLogs.AddRange(logs_jan);



            var logs_anna = new[]
            {
                new PaymentLog { SubscriptionId = sub2_disney.Id, UserId = user2.Id, PaymentDate = new DateTime(2026, 3, 20, 11, 30, 0), AmountPaid = 19.50M, Note = "Opłacenie cyklu Disney+ (Udział 1/2)" },
                new PaymentLog { SubscriptionId = sub2_disney.Id, UserId = user2.Id, PaymentDate = new DateTime(2026, 4, 20, 12, 10, 0), AmountPaid = 19.50M, Note = "Opłacenie cyklu Disney+ (Udział 1/2)" },
                new PaymentLog { SubscriptionId = sub2_disney.Id, UserId = user2.Id, PaymentDate = new DateTime(2026, 5, 20, 09, 0, 0), AmountPaid = 19.50M, Note = "Opłacenie cyklu Disney+ (Udział 1/2)" },


                new PaymentLog { SubscriptionId = sub2_psplus.Id, UserId = user2.Id, PaymentDate = new DateTime(2025, 9, 10, 16, 45, 0), AmountPaid = 400.00M, Note = "Roczna opłata PS Plus Premium" },


                new PaymentLog { SubscriptionId = sub2_yt.Id, UserId = user2.Id, PaymentDate = new DateTime(2026, 4, 5, 10, 0, 0), AmountPaid = 25.99M, Note = "Opłacenie subskrypcji YouTube Premium" },
                new PaymentLog { SubscriptionId = sub2_yt.Id, UserId = user2.Id, PaymentDate = new DateTime(2026, 5, 5, 11, 0, 0), AmountPaid = 25.99M, Note = "Opłacenie subskrypcji YouTube Premium" }
            };
            db.PaymentLogs.AddRange(logs_anna);

            db.SaveChanges();
        }
    }
}
