using System.Text.RegularExpressions;

namespace MoopelApi.Helpers;

public static class RegexHelper
{
    public static bool CheckWithRegex(string input, string pattern)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(pattern);

        return Regex.IsMatch(input, pattern);
    }

    public static bool CheckWithRegex(string input, Regex regex)
    {
        ArgumentNullException.ThrowIfNull(input);

        return regex.IsMatch(input);
    }
}
