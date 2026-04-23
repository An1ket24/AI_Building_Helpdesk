using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'TicketComments', N'U') IS NULL
            BEGIN
                CREATE TABLE [TicketComments] (
                    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [TicketId] int NOT NULL,
                    [Body] nvarchar(max) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] int NOT NULL,
                    CONSTRAINT [FK_TicketComments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_TicketComments_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
                );
                CREATE INDEX [IX_TicketComments_TicketId] ON [TicketComments] ([TicketId]);
            END
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'KnowledgeBaseArticles', N'U') IS NULL
            BEGIN
                CREATE TABLE [KnowledgeBaseArticles] (
                    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Title] nvarchar(450) NOT NULL,
                    [Category] nvarchar(max) NOT NULL,
                    [Guidance] nvarchar(max) NOT NULL,
                    [IsActive] bit NOT NULL DEFAULT 1
                );
                CREATE UNIQUE INDEX [IX_KnowledgeBaseArticles_Title] ON [KnowledgeBaseArticles] ([Title]);
            END
            """);
    }
}
