using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class SignUpRecord
{
    private string _user = null!;
    private string _password = null!;

    static string Salt = "7";

    [StringLength(64, MinimumLength = 3)]
    [RegularExpression(@"^[a-z\d\@_\.-]{3,64}$", MatchTimeoutInMilliseconds = 10)]
    public string User
    {
        get { return _user; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _user = null!;
            }
            if (Regex.IsMatch(value, @"^[a-z\d\@_\.-]{3,64}$", RegexOptions.IgnoreCase))
            {
                _user = value;
            }
        }
    }

    [StringLength(64, MinimumLength = 4)]
    public string Password
    {
        get { return _password; }
        set { _password = GetHash(value, Salt); }
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(User) && !string.IsNullOrWhiteSpace(Password);
    }

    public bool IsEmail()
    {
        return string.IsNullOrEmpty(User) && User.Contains("@");
    }

    static string GetHash(string input, string salt)
    {
        using var sha256Hash = SHA256.Create();
        var buffer = Encoding.ASCII.GetBytes(salt + input);
        var bytes = sha256Hash.ComputeHash(buffer);
        var builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2")); // "x2" for hexadecimal format
        }
        return builder.ToString();
    }
}