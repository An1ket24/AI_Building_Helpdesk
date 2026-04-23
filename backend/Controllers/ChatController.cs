using backend.Dtos;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(IOpenRouterService openRouterService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatAnalysisDto>> Analyze(ChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        var response = await openRouterService.AnalyzeIssueAsync(request.Message, cancellationToken);

        if (User.IsInRole(UserRole.Technician.ToString()) && response.ShouldOfferTicket)
        {
            response = response with
            {
                ShouldOfferTicket = false,
                BotMessage = $"{response.Solution} As a technician, you can use this chat for troubleshooting guidance and update your assigned tickets from the list below."
            };
        }

        return Ok(response);
    }
}
