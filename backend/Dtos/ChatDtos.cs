namespace backend.Dtos;

public record ChatRequest(string Message);

public record ChatAnalysisDto(
    string Issue,
    string Category,
    string? Location,
    string Priority,
    string Solution,
    bool ShouldOfferTicket,
    string BotMessage);
