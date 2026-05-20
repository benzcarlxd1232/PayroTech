using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace PayroTech.Services;

public interface IQRCodeService
{
    string GenerateQRCodeHash(string userId, int companyId);
    string GenerateUniqueCode(string userId);
    bool ValidateQRCodeHash(string hash, string userId, int companyId);
    string GenerateQRCodeDataUrl(string qrContent);
    byte[] GenerateQRCodeBytes(string qrContent, int pixelsPerModule = 20);
}

public class QRCodeService : IQRCodeService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;

    public QRCodeService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["QRCode:SecretKey"] ?? Guid.NewGuid().ToString("N");
    }

    public string GenerateQRCodeHash(string userId, int companyId)
    {
        var timestamp = DateTime.UtcNow.Ticks;
        var data = $"{userId}:{companyId}:{timestamp}:{_secretKey}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        var hash = Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
        
        return hash.Substring(0, Math.Min(32, hash.Length));
    }

    public string GenerateUniqueCode(string userId)
    {
        var timestamp = DateTime.UtcNow.Ticks;
        var data = $"{userId}:{timestamp}:{Guid.NewGuid()}:{_secretKey}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        var hash = Convert.ToBase64String(hashBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
        
        return hash.Substring(0, Math.Min(32, hash.Length));
    }

    public bool ValidateQRCodeHash(string hash, string userId, int companyId)
    {
        return !string.IsNullOrEmpty(hash) && hash.Length == 32;
    }

    public string GenerateQRCodeDataUrl(string qrContent)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        var qrCodeImage = qrCode.GetGraphic(20);
        var base64 = Convert.ToBase64String(qrCodeImage);
        return $"data:image/png;base64,{base64}";
    }

    public byte[] GenerateQRCodeBytes(string qrContent, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(pixelsPerModule);
    }
}
