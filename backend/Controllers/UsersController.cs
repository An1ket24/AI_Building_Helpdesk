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
public class UsersController(AppDbContext context) : ControllerBase
{
    [HttpGet("technicians")]
    public async Task<ActionResult<IEnumerable<UserSummaryResponse>>> GetTechnicians()
    {
        var technicians = await context.Users
            .Where(user => user.Role == UserRole.Technician)
            .OrderBy(user => user.Name)
            .Select(user => new UserSummaryResponse(user.Id, user.Name, user.Email, user.Role.ToString()))
            .ToListAsync();

        return Ok(technicians);
    }
}
