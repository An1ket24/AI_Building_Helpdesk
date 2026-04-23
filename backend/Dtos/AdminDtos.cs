namespace backend.Dtos;

public record UserSummaryResponse(int Id, string Name, string Email, string Role);

public record KnowledgeBaseArticleRequest(string Title, string Category, string Guidance, bool IsActive);

public record KnowledgeBaseArticleResponse(int Id, string Title, string Category, string Guidance, bool IsActive);
