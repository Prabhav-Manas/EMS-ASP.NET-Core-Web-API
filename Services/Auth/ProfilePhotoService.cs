namespace backend.Services.Auth;

public class ProfilePhotoService
{
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 5 * 1024 * 1024;

    public ProfilePhotoService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        if(file.Length == 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        if(file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("Maximum file size is 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowedExtension = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedExtension.Contains(extension))
        {
            throw new InvalidOperationException("Only JPG, PNG and WEBP images are allowed.");
        }

        // Validate actual File Content
        var header = new byte[12];

        await using(var stream = file.OpenReadStream())
        {
            var bytesRead = await stream.ReadAsync(header.AsMemory());

            if(!IsValidImage(header, bytesRead, extension))
            {
                throw new InvalidOperationException("Invalid Image file.");
            }
        }

        var folderPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "profile-photos");

        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        await using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/profile-photos/{fileName}";
    }


    private static bool IsValidImage(byte[] header, int bytesRead, string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" =>
                bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                bytesRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47,

            ".webp" =>
                bytesRead >= 12 &&
                header[0] == 0x52 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x46 &&
                header[8] == 0x57 &&
                header[9] == 0x45 &&
                header[10] == 0x42 &&
                header[11] == 0x50,

            _ => false
        };
    }
}