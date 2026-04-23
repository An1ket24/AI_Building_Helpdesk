namespace backend.Models;

public class KnowledgeBaseArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Guidance { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
