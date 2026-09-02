using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MediCart.Web.Services
{
    public interface IImageUploadService
    {
        Task<string?> UploadMedicineImageAsync(IFormFile file);
    }

    public class CloudinaryImageService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary configuration is missing. Set Cloudinary:CloudName, Cloudinary:ApiKey, " +
                    "Cloudinary:ApiSecret via 'dotnet user-secrets' (dev) or App Service configuration (prod).");
            }

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

public async Task<string?> UploadMedicineImageAsync(IFormFile file)
{
    if (file == null || file.Length == 0)
        return null;

    await using var stream = file.OpenReadStream();

    var uploadParams = new ImageUploadParams
    {
        File = new FileDescription(file.FileName, stream),
        Folder = "medicart/medicines",
        Transformation = new Transformation()
            .Width(600)
            .Height(600)
            .Crop("limit"),
        UseFilename = true,
        UniqueFilename = true,
        Overwrite = false
    };

    try
    {
        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new InvalidOperationException(
                $"Cloudinary error: {result.Error.Message}");
        }

        return result.SecureUrl?.ToString();
    }
    catch (Exception ex)
    {
        Console.WriteLine("========== CLOUDINARY ERROR ==========");
        Console.WriteLine(ex.ToString());
        Console.WriteLine("======================================");

        throw;
    }
}


    }
}