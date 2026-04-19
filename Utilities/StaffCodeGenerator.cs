using Microsoft.EntityFrameworkCore;
using PayroTech.Data;
using PayroTech.Models.Enums;

namespace PayroTech.Utilities;

public static class StaffCodeGenerator
{
    /// <summary>
    /// Generates a sequential staff code: HR-000001, ACC-000001, EMP-000001
    /// Last 4 digits are the default kiosk PIN.
    /// </summary>
    public static async Task<string> GenerateAsync(ApplicationDbContext context, int companyId, UserRole role)
    {
        var prefix = role switch
        {
            UserRole.HR          => "HR",
            UserRole.Accountant  => "ACC",
            UserRole.CompanyAdmin => "MGR",
            _                    => "EMP"
        };

        // Count existing staff codes with this prefix for this company
        var existing = await context.Users
            .Where(u => u.CompanyId == companyId && u.StaffCode.StartsWith(prefix + "-"))
            .CountAsync();

        var seq = existing + 1;
        return $"{prefix}-{seq:D6}"; // e.g. EMP-000001
    }

    /// <summary>
    /// Extracts the default 4-digit PIN from a staff code (last 4 digits of the numeric part).
    /// EMP-000123 → "0123". Falls back to "0000" if code is empty.
    /// </summary>
    public static string GetDefaultPin(string staffCode)
    {
        if (string.IsNullOrEmpty(staffCode)) return "0000";
        var parts = staffCode.Split('-');
        var numeric = parts[^1]; // last segment e.g. "000001"
        // Extract only digits
        var digits = new string(numeric.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return "0000";
        return digits.Length >= 4 ? digits[^4..] : digits.PadLeft(4, '0');
    }
}
