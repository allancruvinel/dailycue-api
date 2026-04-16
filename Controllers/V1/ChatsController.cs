using System.Security.Claims;

using dailycue_api.DTO.Requests;
using dailycue_api.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dailycue_api.Controllers.V1;

[ApiController]
[Authorize]
[Route("v1/chats")]
public class ChatsController(DailyCueContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateChatRequest request)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        var authenticatedUserId = userId.Value;

        var chat = new Chat
        {
            UserId = authenticatedUserId,
            User = null!,
            Name = request.Name?.Trim(),
            Provider = request.Provider?.Trim(),
            WebhookUrl = request.WebhookUrl?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();

        return Created(
            $"/v1/chats/{chat.Id}",
            new
            {
                Message = "Chat criado com sucesso",
                Chat = ToResponse(chat)
            }
        );
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateChatRequest request)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        var authenticatedUserId = userId.Value;

        var chat = await dbContext.Chats.FirstOrDefaultAsync(c => c.Id == id && c.UserId == authenticatedUserId);
        if (chat == null)
        {
            return NotFound(new { Message = "Chat não encontrado" });
        }

        chat.Name = request.Name?.Trim();
        chat.Provider = request.Provider?.Trim();
        chat.WebhookUrl = request.WebhookUrl?.Trim();
        if (request.IsActive.HasValue)
        {
            chat.IsActive = request.IsActive.Value;
        }
        chat.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok(new { Message = "Chat atualizado com sucesso", Chat = ToResponse(chat) });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        var authenticatedUserId = userId.Value;

        var chat = await dbContext.Chats.FirstOrDefaultAsync(c => c.Id == id && c.UserId == authenticatedUserId);
        if (chat == null)
        {
            return NotFound(new { Message = "Chat não encontrado" });
        }

        dbContext.Chats.Remove(chat);
        await dbContext.SaveChangesAsync();

        return Ok(new { Message = "Chat excluído com sucesso" });
    }

    [HttpGet]
    public async Task<IActionResult> GetPaginatedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        var authenticatedUserId = userId.Value;

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Chats
            .AsNoTracking()
            .Where(c => c.UserId == authenticatedUserId)
            .OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync();
        var chats = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Items = chats.Select(ToResponse),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        });
    }

    private Guid? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirstValue("id");
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static object ToResponse(Chat chat)
    {
        return new
        {
            chat.Id,
            chat.UserId,
            chat.Name,
            chat.Provider,
            chat.WebhookUrl,
            chat.IsActive,
            chat.CreatedAt,
            chat.UpdatedAt
        };
    }
}