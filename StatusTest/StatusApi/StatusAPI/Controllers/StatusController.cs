using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StatusAPI.Data;
using StatusAPI.Hubs;
using StatusAPI.Models;
using System.IO;

namespace StatusAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly StatusDbContext _context;
        private readonly IHubContext<StatusHub> _hubContext;
        private readonly IWebHostEnvironment _environment;

        public StatusController(StatusDbContext context, IHubContext<StatusHub> hubContext, IWebHostEnvironment environment)
        {
            _context = context;
            _hubContext = hubContext;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<List<Status>>> GetActiveStatuses()
        {
            var activeStatuses = await _context.Statuses
                .Where(s => s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(activeStatuses);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Status>>> GetUserStatuses(string userId)
        {
            var userStatuses = await _context.Statuses
                .Where(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(userStatuses);
        }

        [HttpPost]
        public async Task<ActionResult<Status>> CreateStatus([FromBody] CreateStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var status = new Status
            {
                UserId = request.UserId,
                UserName = request.UserName,
                Content = request.Content,
                VideoUrl = request.VideoUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();

            // Notify all connected clients in real-time
            await _hubContext.Clients.Group("StatusUpdates")
                .SendAsync("StatusAdded", status);

            return CreatedAtAction(nameof(GetUserStatuses),
                new { userId = status.UserId }, status);
        }

        /// <summary>
        /// Create a status with optional video file upload (multipart/form-data)
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(524288000)] // 500 MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<ActionResult<Status>> UploadAndCreateStatus([FromForm] string userId, [FromForm] string userName, [FromForm] string? content, [FromForm] IFormFile? video)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userName))
                return BadRequest("userId and userName are required");

            string? videoUrl = null;

            if (video != null && video.Length > 0)
            {
                var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                if (!Directory.Exists(uploadsRoot))
                {
                    Directory.CreateDirectory(uploadsRoot);
                }

                var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(video.FileName)}";
                var filePath = Path.Combine(uploadsRoot, safeFileName);
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await video.CopyToAsync(stream);
                }

                var requestScheme = Request.Scheme;
                var requestHost = Request.Host.Value;
                videoUrl = $"{requestScheme}://{requestHost}/uploads/{Uri.EscapeDataString(safeFileName)}";
            }

            var status = new Status
            {
                UserId = userId,
                UserName = userName,
                Content = content,
                VideoUrl = videoUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group("StatusUpdates").SendAsync("StatusAdded", status);

            return CreatedAtAction(nameof(GetUserStatuses), new { userId = status.UserId }, status);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStatus(int id)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
                return NotFound();

            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();

            // Notify all connected clients in real-time
            await _hubContext.Clients.Group("StatusUpdates")
                .SendAsync("StatusDeleted", id);

            return NoContent();
        }

        [HttpDelete("user/{userId}")]
        public async Task<ActionResult> DeleteUserStatuses(string userId)
        {
            var userStatuses = await _context.Statuses
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (!userStatuses.Any())
                return NotFound();

            var deletedIds = userStatuses.Select(s => s.Id).ToList();
            _context.Statuses.RemoveRange(userStatuses);
            await _context.SaveChangesAsync();

            // Notify all connected clients in real-time
            foreach (var id in deletedIds)
            {
                await _hubContext.Clients.Group("StatusUpdates")
                    .SendAsync("StatusDeleted", id);
            }

            return NoContent();
        }
    }
}
