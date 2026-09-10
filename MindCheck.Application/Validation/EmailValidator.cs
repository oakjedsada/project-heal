using System.Text.RegularExpressions;

namespace MindCheck.Application.Validation;

// Deliberately loose — just enough to reject obviously-wrong input
// (no verification email is ever sent, so a stricter RFC 5322 check
// buys nothing here).
internal static partial class EmailValidator
{
    public static bool IsValid(string email) => EmailPattern().IsMatch(email);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
