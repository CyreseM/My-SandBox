namespace ChatApp.Api.Services;

public class FileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _http;

    public FileStorageService(IWebHostEnvironment env, IHttpContextAccessor http)
    {
        _env = env; _http = http;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var filename = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(uploadsDir, filename);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream);

        var request = _http.HttpContext?.Request;
        var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";
        return $"{baseUrl}/uploads/{filename}";
    }
}
