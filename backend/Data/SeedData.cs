using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();

        await EnsureUserAsync(
            context,
            passwordHasher,
            name: "Admin User",
            email: "admin@smarthelpdesk.local",
            password: "Admin123!",
            role: UserRole.Admin);

        await EnsureUserAsync(
            context,
            passwordHasher,
            name: "Demo User",
            email: "user@smarthelpdesk.local",
            password: "User123!",
            role: UserRole.User);

        await EnsureUserAsync(
            context,
            passwordHasher,
            name: "Demo Technician",
            email: "tech@smarthelpdesk.local",
            password: "Tech123!",
            role: UserRole.Technician);

        await EnsureKnowledgeArticleAsync(context, "WiFi Troubleshooting", "IT", "Reconnect to building WiFi, restart the device, forget and rejoin the network, and confirm whether nearby users are affected too.");
        await EnsureKnowledgeArticleAsync(context, "Printer Recovery", "IT", "Check printer power, toner, paper, and network connection. If shared, confirm the device is online on the office network.");
        await EnsureKnowledgeArticleAsync(context, "HVAC Temperature Check", "HVAC", "Confirm thermostat settings, local power, and whether the issue affects one room or a wider area before dispatch.");
        await EnsureKnowledgeArticleAsync(context, "Plumbing Leak Response", "Plumbing", "Keep the area clear, avoid using nearby fixtures, and isolate the water source if safe while maintenance is notified.");
        await EnsureKnowledgeArticleAsync(context, "Cleaning Spill SOP", "Cleaning", "Mark the affected area, keep foot traffic away, and report the exact location and spill type.");
        await EnsureKnowledgeArticleAsync(context, "Access Control Guide", "Security", "Check badge validity, try another approved access point, and confirm whether the issue affects one door or a wider access zone.");
    }

    private static async Task EnsureUserAsync(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        string name,
        string email,
        string password,
        UserRole role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingUser = await context.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail);
        if (existingUser is not null)
        {
            existingUser.Name = name;
            existingUser.Role = role;
            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, password);
            await context.SaveChangesAsync();
            return;
        }

        var user = new User
        {
            Name = name,
            Email = normalizedEmail,
            Role = role
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureKnowledgeArticleAsync(AppDbContext context, string title, string category, string guidance)
    {
        var existingArticle = await context.KnowledgeBaseArticles.FirstOrDefaultAsync(article => article.Title == title);
        if (existingArticle is not null)
        {
            if (!existingArticle.IsActive)
            {
                existingArticle.IsActive = true;
                existingArticle.Category = category;
                existingArticle.Guidance = guidance;
                await context.SaveChangesAsync();
            }

            return;
        }

        context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
        {
            Title = title,
            Category = category,
            Guidance = guidance,
            IsActive = true
        });

        await context.SaveChangesAsync();
    }
}
