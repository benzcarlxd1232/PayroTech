using System.Security.Cryptography;
using System.Text;

namespace PayroTech.Services;

public interface IPasswordGeneratorService
{
    /// <summary>
    /// Generate a random temporary password for managers (SuperAdmin creates these)
    /// </summary>
    string GenerateRandomPassword(int length = 12);

    /// <summary>
    /// Generate a birthday-based temporary password for HR, Accountant, and Employees
    /// Format: MMDDYYYY@FirstName (e.g., 11092001@John)
    /// </summary>
    string GenerateBirthdayPassword(DateTime birthDate, string firstName);
    
    /// <summary>
    /// Generate a name@year password for HR and Accountant
    /// Format: FirstName@BirthYear (e.g., Benz@2005)
    /// </summary>
    string GenerateNameYearPassword(string firstName, int birthYear);
}

public class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%^&*";

    public string GenerateRandomPassword(int length = 12)
    {
        var allChars = UpperCase + LowerCase + Digits + Special;
        var password = new StringBuilder();

        // Ensure at least one of each required character type
        password.Append(UpperCase[RandomNumberGenerator.GetInt32(UpperCase.Length)]);
        password.Append(LowerCase[RandomNumberGenerator.GetInt32(LowerCase.Length)]);
        password.Append(Digits[RandomNumberGenerator.GetInt32(Digits.Length)]);
        password.Append(Special[RandomNumberGenerator.GetInt32(Special.Length)]);

        // Fill the rest with random characters
        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
        }

        // Shuffle the password
        return ShuffleString(password.ToString());
    }

    public string GenerateBirthdayPassword(DateTime birthDate, string firstName)
    {
        // Format: MMDDYYYY@FirstName (e.g., 11092001@John)
        var dateString = birthDate.ToString("MMddyyyy");
        var cleanFirstName = firstName.Trim().Replace(" ", "");
        
        // Capitalize first letter of name
        if (!string.IsNullOrEmpty(cleanFirstName))
        {
            cleanFirstName = char.ToUpper(cleanFirstName[0]) + cleanFirstName.Substring(1).ToLower();
        }

        return $"{dateString}@{cleanFirstName}";
    }
    
    public string GenerateNameYearPassword(string firstName, int birthYear)
    {
        // Trim whitespace
        var cleanFirstName = firstName?.Trim() ?? string.Empty;
        
        if (string.IsNullOrEmpty(cleanFirstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        // Extract first word (before first space) for multi-word names
        var firstWord = cleanFirstName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        
        // Capitalize first letter, lowercase rest
        if (!string.IsNullOrEmpty(firstWord))
        {
            firstWord = char.ToUpper(firstWord[0]) + firstWord.Substring(1).ToLower();
        }

        return $"{firstWord}@{birthYear}";
    }

    private static string ShuffleString(string str)
    {
        var array = str.ToCharArray();
        var n = array.Length;
        
        for (int i = n - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        
        return new string(array);
    }
}
