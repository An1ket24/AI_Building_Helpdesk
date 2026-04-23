using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class KnowledgeBaseController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseArticleResponse>>> GetAll()
    {
        var articles = await context.KnowledgeBaseArticles
            .OrderBy(article => article.Category)
            .ThenBy(article => article.Title)
            .Select(article => new KnowledgeBaseArticleResponse(article.Id, article.Title, article.Category, article.Guidance, article.IsActive))
            .ToListAsync();

        return Ok(articles);
    }

    [HttpPost]
    public async Task<ActionResult<KnowledgeBaseArticleResponse>> Create(KnowledgeBaseArticleRequest request)
    {
        var article = new KnowledgeBaseArticle
        {
            Title = request.Title.Trim(),
            Category = request.Category.Trim(),
            Guidance = request.Guidance.Trim(),
            IsActive = request.IsActive
        };

        context.KnowledgeBaseArticles.Add(article);
        await context.SaveChangesAsync();

        return Ok(new KnowledgeBaseArticleResponse(article.Id, article.Title, article.Category, article.Guidance, article.IsActive));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<KnowledgeBaseArticleResponse>> Update(int id, KnowledgeBaseArticleRequest request)
    {
        var article = await context.KnowledgeBaseArticles.FindAsync(id);
        if (article is null)
        {
            return NotFound();
        }

        article.Title = request.Title.Trim();
        article.Category = request.Category.Trim();
        article.Guidance = request.Guidance.Trim();
        article.IsActive = request.IsActive;
        await context.SaveChangesAsync();

        return Ok(new KnowledgeBaseArticleResponse(article.Id, article.Title, article.Category, article.Guidance, article.IsActive));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await context.KnowledgeBaseArticles.FindAsync(id);
        if (article is null)
        {
            return NotFound();
        }

        context.KnowledgeBaseArticles.Remove(article);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
