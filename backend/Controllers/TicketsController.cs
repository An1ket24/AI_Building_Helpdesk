using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController(AppDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(CreateTicketRequest request)
    {
        if (User.IsInRole(UserRole.Technician.ToString()))
        {
            return Forbid();
        }

        var userId = GetUserId();
        var ticket = new Ticket
        {
            Issue = request.Issue,
            Category = request.Category,
            Location = request.Location,
            Priority = request.Priority,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var user = await context.Users.FindAsync(userId);
        return Ok(MapTicket(ticket, user?.Name, CanCurrentUserUpdate(ticket)));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAll()
    {
        var userId = GetUserId();
        var userEmail = GetUserEmail();
        var isAdmin = User.IsInRole(UserRole.Admin.ToString());
        var isTechnician = User.IsInRole(UserRole.Technician.ToString());

        var query = context.Tickets
            .Include(ticket => ticket.CreatedByUser)
            .AsQueryable();

        if (!isAdmin)
        {
            query = isTechnician
                ? query.Where(ticket => ticket.AssignedTo == userEmail || ticket.CreatedBy == userId)
                : query.Where(ticket => ticket.CreatedBy == userId);
        }

        var tickets = await query
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync();

        return Ok(tickets.Select(ticket => MapTicket(ticket, ticket.CreatedByUser?.Name, CanCurrentUserUpdate(ticket))));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Technician")]
    public async Task<ActionResult<TicketResponse>> Update(int id, UpdateTicketRequest request)
    {
        var ticket = await context.Tickets.Include(item => item.CreatedByUser).FirstOrDefaultAsync(item => item.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole(UserRole.Admin.ToString());
        if (!isAdmin)
        {
            if (!IsAssignedTechnician(ticket))
            {
                return Forbid();
            }

            ticket.Status = string.IsNullOrWhiteSpace(request.Status) ? ticket.Status : request.Status;
        }
        else
        {
            ticket.Priority = string.IsNullOrWhiteSpace(request.Priority) ? ticket.Priority : request.Priority;
            ticket.AssignedTo = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo.Trim();
        }

        await context.SaveChangesAsync();
        return Ok(MapTicket(ticket, ticket.CreatedByUser?.Name, CanCurrentUserUpdate(ticket)));
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<TicketCommentResponse>>> GetComments(int id)
    {
        var ticket = await context.Tickets.FindAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!CanAccessTicket(ticket))
        {
            return Forbid();
        }

        var comments = await context.TicketComments
            .Include(comment => comment.CreatedByUser)
            .Where(comment => comment.TicketId == id)
            .OrderBy(comment => comment.CreatedAt)
            .Select(comment => new TicketCommentResponse(
                comment.Id,
                comment.Body,
                AsUtc(comment.CreatedAt),
                comment.CreatedBy,
                comment.CreatedByUser != null ? comment.CreatedByUser.Name : null,
                comment.CreatedByUser != null ? comment.CreatedByUser.Role.ToString() : null))
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(int id, CreateTicketCommentRequest request)
    {
        var ticket = await context.Tickets.FindAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!CanAccessTicket(ticket))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { message = "Comment body is required." });
        }

        var userId = GetUserId();
        var user = await context.Users.FindAsync(userId);

        var comment = new TicketComment
        {
            TicketId = id,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        context.TicketComments.Add(comment);
        await context.SaveChangesAsync();

        return Ok(new TicketCommentResponse(comment.Id, comment.Body, AsUtc(comment.CreatedAt), comment.CreatedBy, user?.Name, user?.Role.ToString()));
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? throw new InvalidOperationException("Missing user id claim."));
    }

    private string GetUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? string.Empty;
    }

    private bool IsAssignedTechnician(Ticket ticket)
    {
        return User.IsInRole(UserRole.Technician.ToString())
            && !string.IsNullOrWhiteSpace(ticket.AssignedTo)
            && string.Equals(ticket.AssignedTo, GetUserEmail(), StringComparison.OrdinalIgnoreCase);
    }

    private bool CanAccessTicket(Ticket ticket)
    {
        if (User.IsInRole(UserRole.Admin.ToString()))
        {
            return true;
        }

        if (ticket.CreatedBy == GetUserId())
        {
            return true;
        }

        return IsAssignedTechnician(ticket);
    }

    private bool CanCurrentUserUpdate(Ticket ticket)
    {
        return User.IsInRole(UserRole.Admin.ToString()) || IsAssignedTechnician(ticket);
    }

    private static TicketResponse MapTicket(Ticket ticket, string? createdByName, bool canCurrentUserUpdate)
    {
        return new TicketResponse(
            ticket.Id,
            ticket.Issue,
            ticket.Category,
            ticket.Location,
            ticket.Priority,
            ticket.Status,
            AsUtc(ticket.CreatedAt),
            ticket.CreatedBy,
            createdByName,
            ticket.AssignedTo,
            canCurrentUserUpdate);
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
